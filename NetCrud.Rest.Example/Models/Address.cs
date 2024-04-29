using NetCrud.Rest.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NetCrud.Rest.Example.Models
{
    public class Address : EntityBase
    {
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        [JsonIgnore]
        public Customer Customer { get; set; }
        public string AddressText { get; set; }
    }
}
