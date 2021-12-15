using Nv.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Nv.Example.Models
{
    public class Address : EntityBase
    {
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        public string AddressText { get; set; }
    }
}
