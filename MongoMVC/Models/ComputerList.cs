using System.Collections.Generic;

namespace MongoMVC.Models
{
    public class ComputerList
    {
        public IEnumerable<Computer> Computers { get; set; }
        public ComputerFilter Filter { get; set; }
    }
}
