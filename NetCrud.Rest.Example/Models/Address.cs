using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NetCrud.Rest.Example.Models
{
    public class Address : CrudEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        public string AddressText { get; set; }
    }
}
