//using Microsoft.AspNetCore.JsonPatch;
//using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetCrud.Rest.Data;
using NetCrud.Rest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCrud.Rest.Core
{
    public class EntityService<TEntity, TId> : IEntityService<TEntity, TId> where TEntity : EntityBase<TId>
    {
        protected readonly IRepository<TEntity> repository;
        protected readonly IUnitOfWork unitOfWork;

        public EntityService(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
        }
        public virtual Task After(IList<TEntity> entities, ServiceActionType actionType, object data = null)
        {
            return Task.CompletedTask;
        }

      

        public virtual Task<object> Before(IList<TEntity> entities, ServiceActionType actionType)
        {
            object obj = null;
            return Task.FromResult(obj);
        }

        public async Task<TEntity> Create(TEntity entity)
        {
            var data = await this.Before(new List<TEntity> { entity }, ServiceActionType.Create);
            
            await repository.AddAsync(entity, true);
            await unitOfWork.CommitAsync();

            await this.After(new List<TEntity> { entity }, ServiceActionType.Create, data);

            return entity;
        }

        public async Task<TEntity> Delete(object id)
        {
            var entity = await repository.FindByIdAsync(id);
            if (entity != null)
            {
                var data = await this.Before(new List<TEntity> { entity }, ServiceActionType.Delete);
                repository.Delete(entity);
                await unitOfWork.CommitAsync();
                await this.After(new List<TEntity> { entity }, ServiceActionType.Delete, data);
                return entity;
            }
            else return null;
        }

        public async Task<TEntity> Get(object id, bool forUpdate = false, string[] naviations = null)
        {
            var entity = await repository.FindByIdAsync(id, forUpdate, naviations);
            await this.After(new List<TEntity> { entity }, ServiceActionType.Read);
            return entity;
        }

        public async Task<IList<TEntity>> GetAll(GetAllQueryStringParameters<TEntity> request)
        {
            var entities = await repository.FindAsync(q => request.ApplyFilter(q), q => request.ApplySort(q), request.GetIncludes());
            return entities;
        }

        public async Task<IPagedList<TEntity>> GetAllPaged(GetAllQueryStringParameters<TEntity> request)
        {
            var entities = await repository.FindPagedAsync(q => request.ApplyFilter(q), q => request.ApplySort(q), request.PageNumber - 1, request.PageSize, false, request.GetIncludes());
            await this.After(entities, ServiceActionType.Read);
            return entities;
        }

        public async Task<TEntity> Update(TEntity entity)
        {
            object data = await this.Before(new List<TEntity> { entity }, ServiceActionType.Update);
            repository.Update(entity);
            await unitOfWork.CommitAsync();
            await this.After(new List<TEntity> { entity }, ServiceActionType.Update, data);
            return entity;
        }

       
    }

    public class EntityService<TEntity> : EntityService<TEntity, int> where TEntity : EntityBase<int>
    {
        public EntityService(IRepository<TEntity> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {

        }
    }
}
