using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
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
    }
}
