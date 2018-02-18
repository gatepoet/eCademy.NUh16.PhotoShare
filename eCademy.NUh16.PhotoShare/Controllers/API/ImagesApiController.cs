using eCademy.NUh16.PhotoShare.Models;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Formatting;

namespace eCademy.NUh16.PhotoShare.Controllers.API
{
    public class ImagesApiController : ApiController
    {
        private ApplicationDbContext _db;
        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationDbContext>();
            }
            private set
            {
                _db = value;
            }
        }
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        [Route("api/Images")]
        public IHttpActionResult Get()
        {
            var images = Db.Images
                .OrderByDescending(image => image.Timestamp)
                .Take(9)
                .ToArray()
                .Select(image => new ImageListItemDto
                {
                    Id = image.Id,
                    Title = image.Title,
                    ImageUrl = Url.Content($"~/Images/Uploads/{image.Id}"),
                    PhotoUrl = Url.Route("Default", new {
                        controller = "Images",
                        action = "Details",
                        id = image.Id
                    })
                });

            return Ok(images);
        }

        [HttpPost]
        [Route("api/Images")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> Post()
        {
            var owner = await UserManager.FindByNameAsync(User.Identity.Name);

            var request = HttpContext.Current.Request;

            var file = new UploadedFile
            {
                Id = Guid.NewGuid(),
                Filename = request.Files[0].FileName,
                ImageData = ReadFile(request.Files[0]),
            };

            var image = new Image
            {
                Timestamp = DateTime.Now,
                Title = request.Form["title"],
                File = file,
                User = owner,
            };

            if (string.IsNullOrWhiteSpace (image.Title) || request.Files.Count == 0)
            {
                return BadRequest();
            }

            Db.Images.Add(image);
            Db.SaveChanges();

            return Ok(image.Id);
        }

        [HttpPost]
        [Authorize]
        [Route("api/photos/uploadMobile")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> PostPhotoMobile(UploadPhotoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var owner = await UserManager.FindByNameAsync(User.Identity.Name);

            var file = new UploadedFile
            {
                Id = Guid.NewGuid(),
                Filename = request.Filename,
                ImageData = request.File,
            };

            var image = new Image
            {
                Timestamp = DateTime.Now,
                Title = request.Title,
                File = file,
                User = owner,
            };

            Db.Images.Add(image);
            Db.SaveChanges();

            return Ok(image.Id);
        }
        private byte[] ReadFile(HttpPostedFile file)
        {
            byte[] imageData;
            using (var memoryStream = new MemoryStream(file.ContentLength))
            {
                file.InputStream.CopyTo(memoryStream);
                imageData = memoryStream.ToArray();
            }

            return imageData;
        }
        // PUT api/<controller>/5
        [HttpPut]
        [Route("Images/{id:int}/rate/{rating:int}")]
        public async Task<IHttpActionResult> PutRate(int id, int rating)
        {
            var image = Db.Images.Find(id);
            if (image == null)
            {
                return NotFound();
            }
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            image.Rate(rating, user);
            await Db.SaveChangesAsync();

            return Ok(new RateResult(image.GetScore()));
        }
    }

    public class UploadPhotoRequest
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Filename { get; set; }
        [Required]
        public byte[] File { get; set; }
    }

    internal class ImageListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Base64Image { get; set; }
        public string PhotoUrl { get; internal set; }
        public string ImageUrl { get; internal set; }
    }
}