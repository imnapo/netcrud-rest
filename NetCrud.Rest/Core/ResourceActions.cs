using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetCrud.Rest.Core
{
    public class ResourceActions<TEntity, TId> : IResourceActions<TEntity> where TEntity : EntityBase<TId>
    {
        public virtual void AfterCreate(TEntity entity)
        {
            return;
        }

        public virtual void AfterUpdate(TEntity entity)
        {
            return;
        }

        public virtual void BeforeCreate(TEntity entity)
        {
            return;
        }

        public virtual void BeforeUpdate(TEntity entity)
        {
            return;
        }
    }

    public class ResourceActions<TEntity> : ResourceActions<TEntity, int> where TEntity : EntityBase<int>
    {

    }
}
