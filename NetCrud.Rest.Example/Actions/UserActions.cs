using Microsoft.AspNetCore.Http;
using NetCrud.Rest.Core;
using NetCrud.Rest.Example.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCrud.Rest.Example.Definations
{
    public class UserActions : ResourceActions<User>
    {
        private readonly HttpContext httpContext;

        public UserActions(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public override void BeforeCreate(User entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;
        }

        public override void BeforeUpdate(User entity)
        {
            entity.ModifiedAt = DateTime.UtcNow;
        }

    }
}
