using Microsoft.AspNetCore.Http;
using NetCrud.Rest.Core;
using NetCrud.Rest.Data;
using NetCrud.Rest.Example.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCrud.Rest.Example.Definations
{
    public class UserService : EntityService<User>
    {
        public UserService(IRepository<User> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
        {
        }

        public override Task<User> BeforeWrite(User entity, ServiceActionType actionType)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;
            return base.BeforeWrite(entity, actionType);
        }


    }
}
