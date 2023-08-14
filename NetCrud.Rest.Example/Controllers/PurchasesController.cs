using NetCrud.Rest.Example.Models;
using Microsoft.AspNetCore.Mvc;
using NetCrud.Rest.Controllers;
using NetCrud.Rest.Core;

namespace NetCrud.Rest.Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : CrudControllerBase<Purchase>
    {
        public PurchasesController(IEntityService<Purchase, int> service, IDataShaper<Purchase> dataShaper) : base(service, dataShaper)
        {
        }
    }
}
