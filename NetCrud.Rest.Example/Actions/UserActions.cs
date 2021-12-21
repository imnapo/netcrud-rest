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

        public override Task BeforeCreateAsync(User entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;
            return base.BeforeCreateAsync(entity);
        }

        public override Task BeforeUpdateAsync(User entity)
        {
            entity.ModifiedAt = DateTime.UtcNow;
            return base.BeforeUpdateAsync(entity);
        }

    }
}
