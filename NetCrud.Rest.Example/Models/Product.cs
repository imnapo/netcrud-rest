using NetCrud.Rest.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NetCrud.Rest.Example.Models
{
    public class Product : EntityBase<int>
    {
        public string Name { get; set; }

        public double Price { get; set; }

        [JsonIgnore]
        public ICollection<Purchase> Purchases { get; set; }
    }
}
