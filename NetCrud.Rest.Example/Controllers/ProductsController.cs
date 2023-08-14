using AutoMapper;
using NetCrud.Rest.Example.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCrud.Rest.Controllers;
using NetCrud.Rest.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCrud.Rest.Core;

namespace NetCrud.Rest.Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : CrudControllerBase<Product>
    {
        public ProductsController(IEntityService<Product, int> service, IDataShaper<Product> dataShaper) : base(service, dataShaper)
        {
        }
    }
}
