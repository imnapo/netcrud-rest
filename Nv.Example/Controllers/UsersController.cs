using AutoMapper;
using Nv.Example.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nv.AspNetCore.Controllers;
using Nv.AspNetCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nv.Example.Controllers
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
