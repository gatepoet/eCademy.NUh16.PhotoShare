using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eCademy.NUh16.PhotoShare.Models;
using System.IO;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Net.Mime;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Configuration;

namespace eCademy.NUh16.PhotoShare.Controllers
{
    public partial class ImagesController : Controller
    {
        private ApplicationDbContext _db;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? HttpContext.GetOwinContext().GetUserManager<ApplicationDbContext>();
            }
            private set
            {
                _db = value;
            }
        }

        [Route("Images/Uploads/{id:int}")]
        [OutputCache(VaryByParam = "*", CacheProfile = "ImageCache")]
        public FileResult Uploads(int id, int? thumb = null)
        {
            var file = Db.Images.Find(id)?.File;
            
            return File(
                ImageHelper.ResizeImage(file.ImageData, thumb),
                ImageHelper.GetContentType(file.Filename),
                file.Filename);
        }

        // GET: Images
        public ActionResult Index()
        {
            var images = Db.Images
                .Include(image => image.File)
                .ToList();
            return View(images);
        }

        // GET: Images/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = Db.Images
                .Where(image => image.Id == id)
                .Single();
            var userId = User.Identity.GetUserId();
            var imageViewModel = new ImageViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Timestamp = item.Timestamp,
                ImageUrl = Url.Content($"~/Images/Uploads/{item.Id}"),
                Score = item.GetScore(),
                Rating = item.GetRating(userId),
                Username = item.User.UserName,
                Email = item.User.Email,
                IsOwner = item.User.Id == userId,
            };

            if (imageViewModel == null)
            {
                return HttpNotFound();
            }
            return View(imageViewModel);
        }

        // GET: Images/Upload
        [Authorize]
        public ActionResult Upload()
        {
            return View();
        }

        // GET: Images/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = Db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // POST: Images/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "Id,Title,Timestamp,ImageData")] Image image)
        {
            if (ModelState.IsValid)
            {
                Db.Entry(image).State = EntityState.Modified;
                Db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(image);
        }

        // GET: Images/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = Db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Image image = Db.Images.Find(id);
            Db.Images.Remove(image);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UserManager.Dispose();
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
