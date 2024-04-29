//using Microsoft.AspNetCore.JsonPatch;
//using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetCrud.Rest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCrud.Rest.Core
{
    public interface IEntityService<TEntity, TId> where TEntity : EntityBase<TId>
    {
        public Task<object> Before(IList<TEntity> entities, ServiceActionType actionType);
        public Task After(IList<TEntity> entities, ServiceActionType actionType, object data);

        public Task<TEntity> Create(TEntity entity);
        public Task<TEntity> Update(TEntity entity);
        public Task<TEntity> Delete(object id);
        public Task<TEntity> Get(object id, bool forUpdate =false, string[]? naviations = null);
        public Task<IList<TEntity>> GetAll(GetAllQueryStringParameters<TEntity> request);
        public Task<IPagedList<TEntity>> GetAllPaged(GetAllQueryStringParameters<TEntity> request);
    }

    //public interface IEntityService<TEntity> : IEntityService<TEntity,int> where TEntity : EntityBase<int>
    //{
    //}

    public enum ServiceActionType : byte
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
    }
}
