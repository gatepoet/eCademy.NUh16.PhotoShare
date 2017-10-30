using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eCademy.NUh16.PhotoShare.Models;
using System.IO;

namespace eCademy.NUh16.PhotoShare.Controllers
{
    public partial class ImagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Images
        public ActionResult Index()
        {
            var images = db.Images
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
            var item = db.Images
                .Where(image => image.Id == id)
                .Select(image => new
                {
                    Id = image.Id,
                    Title = image.Title,
                    Timestamp = image.Timestamp,
                    ImageData = image.File.ImageData,
                })
                .Single();
            var imageViewModel = new ImageViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Timestamp = item.Timestamp,
                Base64Image = "data:image/png;base64," + Convert.ToBase64String(item.ImageData)
            };

            if (imageViewModel == null)
            {
                return HttpNotFound();
            }
            return View(imageViewModel);
        }

        // GET: Images/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(NewImage viewModel)
        {
            byte[] imageData;
            using (var memoryStream = new MemoryStream(viewModel.File.ContentLength))
            {
                viewModel.File.InputStream.CopyTo(memoryStream);
                imageData = memoryStream.ToArray();
            }
            var file = new UploadedFile
            {
                Id = Guid.NewGuid(),
                Filename = viewModel.File.FileName,
                ImageData = imageData,
            };

            var image = new Image
            {
                Timestamp = DateTime.Now,
                Title = viewModel.Title,
                File = file,
            };
            image.Timestamp = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Images.Add(image);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(image);
        }

        // GET: Images/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
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
                db.Entry(image).State = EntityState.Modified;
                db.SaveChanges();
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
            Image image = db.Images.Find(id);
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
            Image image = db.Images.Find(id);
            db.Images.Remove(image);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
