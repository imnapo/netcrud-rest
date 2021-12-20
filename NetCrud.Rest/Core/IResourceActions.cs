using System;
using System.Collections.Generic;
using System.Text;

namespace NetCrud.Rest.Core
{
    public interface IResourceActions<TEntity>
    {
        public void BeforeCreate(TEntity entity);
        public void AfterCreate(TEntity entity);

        public void BeforeUpdate(TEntity entity);
        public void AfterUpdate(TEntity entity);
    }
}
