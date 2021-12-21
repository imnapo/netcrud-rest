using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetCrud.Rest.Core
{
    public interface IResourceActions<TEntity>
    {
        public Task BeforeCreateAsync(TEntity entity);
        public Task AfterCreateAsync(TEntity entity);

        public Task BeforeUpdateAsync(TEntity entity);
        public Task AfterUpdateAsync(TEntity entity);

        public Task BeforeDeleteAsync(object id);
        public Task AfterDeleteAsync(object id);
    }
}
