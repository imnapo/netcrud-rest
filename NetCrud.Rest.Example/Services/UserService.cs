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
    public class PurchaseService : EntityService<Purchase, int>
    {
        public PurchaseService(IRepository<Purchase> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
        {
        }

        public override Task<object> Before(IList<Purchase> entities, ServiceActionType actionType)
        {
            foreach (var entity in entities)
            {
                entity.PurchaseDate = DateTime.UtcNow;

            }
            return base.Before(entities, actionType);
        }


    }
}
