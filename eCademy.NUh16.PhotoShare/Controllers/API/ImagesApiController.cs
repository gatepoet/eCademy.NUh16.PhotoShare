using eCademy.NUh16.PhotoShare.Models;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Linq;
using System;

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
                    ImageUrl = "/Images/Uploads/" + image.Id,
                    PhotoUrl = Url.Route("Default", new {
                        controller = "Images",
                        action = "Details",
                        id = image.Id
                    })
                });

            return Ok(images);
        }

        // PUT api/<controller>/5
        [HttpPut]
        [Route("Images/{id:int}/rate/{rating:int}")]
        public IHttpActionResult PutRate(int id, int rating)
        {
            ///TODO: Rate photo, return new score.
            var newScore = 1.1;

            return Ok(new RateResult(newScore));
        }
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