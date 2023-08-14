using NetCrud.Rest.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCrud.Rest.Example.Models
{
    public class Purchase : EntityBase
    {
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        public DateTime PurchaseDate { get; set; }

    }
}
