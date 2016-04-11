using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoMVC.Models;

namespace MongoMVC.Controllers
{
    public class HomeController : Controller
    {
        ComputerContext db = new ComputerContext();

        public async Task<ActionResult> Index(ComputerFilter cFilter)
        {
            var computers = await FilterAsync(cFilter);
            var model = new ComputerList { Computers = computers, Filter = cFilter };
            return View(model);
        }

        public async Task<IEnumerable<Computer>> FilterAsync(ComputerFilter cFilter)
        {
            var builder = Builders<Computer>.Filter;
            var filters = new List<FilterDefinition<Computer>>();
            if (!String.IsNullOrWhiteSpace(cFilter.ComputerName))
            {
                filters.Add(builder.Eq("Name", new BsonRegularExpression(cFilter.ComputerName)));
            }
            if (cFilter.Year.HasValue)
            {
                filters.Add(builder.Eq("Year", cFilter.Year));
            }
            return await db.Computers.Find(builder.And(filters)).ToListAsync();
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Computer c)
        {
            if (ModelState.IsValid)
            {
                await db.Computers.InsertOneAsync(c);
                return RedirectToAction("Index");
            }
            return View(c);
        }
        public async Task<ActionResult> Edit(string id)
        {
            Computer c = await db.Computers
                .Find(new BsonDocument("_id", new ObjectId(id)))
                .FirstOrDefaultAsync();
            if (c == null)
                return HttpNotFound();
            return View(c);
        }
        [HttpPost]
        public async Task<ActionResult> Edit(Computer c)
        {
            if (ModelState.IsValid)
            {
                await db.Computers.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(c.Id)), c);
                return RedirectToAction("Index");
            }
            return View(c);
        }
        public async Task<ActionResult> Delete(string id)
        {
            await db.Computers.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> AttachImage(string id)
        {
            Computer c = await db.Computers
                .Find(new BsonDocument("_id", new ObjectId(id)))
                .FirstOrDefaultAsync();
            if (c == null)
                return HttpNotFound();
            return View(c);
        }

        [HttpPost]
        public async Task<ActionResult> AttachImage(string id, HttpPostedFileBase file)
        {
            Computer c = await db.Computers
                .Find(new BsonDocument("_id", new ObjectId(id)))
                .FirstOrDefaultAsync();
            if (c.HasImage())
            {
                await DeleteImage(c);
            }
            await StoreImage(c, file);
            return RedirectToAction("Index");
        }

        private async Task StoreImage(Computer c, HttpPostedFileBase file)
        {
            //            var imageId = ObjectId.GenerateNewId();
            var imageId = await db.GridFS.UploadFromStreamAsync(file.FileName, file.InputStream, new GridFSUploadOptions()
            {
                Metadata = new BsonDocument("ContentType", file.ContentType)
            });
            c.ImageId = imageId.ToString();
            var filter = Builders<Computer>.Filter.Eq("_id", new ObjectId(c.Id));
            var update = Builders<Computer>.Update.Set("ImageId", c.ImageId);
            await db.Computers.UpdateOneAsync(filter, update);
//            db.GridFS.Upload(file.InputStream, file.FileName, new MongoGridFSCreateOptions
        }

        private async Task DeleteImage(Computer c)
        {
//            db.GridFS.DeleteById(new ObjectId(c.ImageId));
            db.GridFS.Delete(new ObjectId(c.ImageId));
            c.ImageId = null;
            var filter = Builders<Computer>.Filter.Eq("_id", new ObjectId(c.Id));
            var update = Builders<Computer>.Update.Set("ImageId", c.ImageId);
            await db.Computers.UpdateOneAsync(filter, update);
        }
        public ActionResult GetImage(string id)
        {
            //            var image = db.GridFS.FindOneById(new ObjectId(id));

            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", new ObjectId(id));
            var image = db.GridFS.Find(filter);
            image.MoveNext();
            var currentImage = image.Current.FirstOrDefault();
            if (currentImage == null)
            {
                return HttpNotFound();
            }
//            return File(image.OpenRead(), image.ContentType);

            return File(db.GridFS.DownloadAsBytes(new ObjectId(id)), currentImage.Metadata["ContentType"].AsString);
        }
    }
}
