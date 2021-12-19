using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetCrud.Rest.Core
{
    public class ResourceActions<TEntity> : IResourceActions<TEntity> where TEntity : EntityBase
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
}
