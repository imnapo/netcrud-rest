using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetCrud.Rest.Core
{
    public class ResourceActions<TEntity, TId> : IResourceActions<TEntity> where TEntity : EntityBase<TId>
    {
        public virtual Task AfterCreateAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public Task AfterDeleteAsync(object id)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterUpdateAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public virtual Task BeforeCreateAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        public Task BeforeDeleteAsync(object id)
        {
            return Task.CompletedTask;
        }

        public virtual Task BeforeUpdateAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }
    }

    public class ResourceActions<TEntity> : ResourceActions<TEntity, int> where TEntity : EntityBase<int>
    {

    }
}
