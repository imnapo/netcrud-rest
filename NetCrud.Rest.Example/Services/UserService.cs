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
    public class UserService : EntityService<User, int>
    {
        public UserService(IRepository<User> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
        {
        }

        public override Task<object> Before(IList<User> entities, ServiceActionType actionType)
        {
            foreach (var entity in entities)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.ModifiedAt = DateTime.UtcNow;

            }
            return base.Before(entities, actionType);
        }


    }
}
