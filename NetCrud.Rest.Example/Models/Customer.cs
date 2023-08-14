using NetCrud.Rest.Core;
using NetCrud.Rest.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCrud.Rest.Example.Models
{
    public class Customer : EntityBase<int>
    {
        public string Name { get; set; }

        public int Age { get; set; }

        //[ForeignKey("Address")]
        //public int? AddressId { get; set; }

        [EagerLoad]
        public Address Address { get; set; }

        public ICollection<Purchase> Purchases { get; set; }
    }
}
