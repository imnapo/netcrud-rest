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

namespace NetCrud.Rest.Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : CrudControllerBase<User, int, UserParameters>
    {
        public UsersController(IRepository<User> repository, IUnitOfWork unitOfWork, IMapper mapper) : base(repository, unitOfWork)
        {
        }
    }
}
