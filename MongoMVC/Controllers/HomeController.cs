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
    }
}