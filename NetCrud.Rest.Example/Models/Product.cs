using NetCrud.Rest.Models;
using System.Collections.Generic;

namespace NetCrud.Rest.Example.Models
{
    public class Product : EntityBase<int>
    {
        public string Name { get; set; }

        public double Price { get; set; }

        public ICollection<Purchase> Purchases { get; set; }
    }
}
