using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetCrud.Rest.Data;
using NetCrud.Rest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCrud.Rest.Core
{
    public class EntityService<TEntity, TId> : IEntityService<TEntity, TId> where TEntity : EntityBase<TId>
    {
        private readonly IRepository<TEntity> repository;
        private readonly IUnitOfWork unitOfWork;

        public EntityService(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
        }
        public virtual Task<TEntity> AfterWrite(TEntity entity, ServiceActionType actionType)
        {
            return Task.FromResult(entity);
        }

        public virtual Task<TEntity> BeforeWrite(TEntity entity, ServiceActionType actionType)
        {
            return Task.FromResult(entity);
        }

        public async Task<TEntity> Create(TEntity entity)
        {
            entity = await this.BeforeWrite(entity, ServiceActionType.Create);

            await repository.AddAsync(entity);
            await unitOfWork.CommitAsync();

            entity = await this.AfterWrite(entity, ServiceActionType.Create);

            return entity;
        }

        public async Task<TEntity> Delete(object id)
        {
            var entity = await repository.FindByIdAsync(id);
            if (entity != null)
            {
                repository.Delete(entity);
                await unitOfWork.CommitAsync();
                return entity;
            }
            else return null;
        }

        public async Task<TEntity> Get(object id, bool forUpdate = false, string[] naviations = null)
        {
            return await repository.FindByIdAsync(id, forUpdate, naviations);
        }

        public async Task<IList<TEntity>> GetAll(GetAllQueryStringParameters<TEntity> request)
        {
            var entities = await repository.FindAsync(q => request.ApplyFilter(q), q => request.ApplySort(q), request.GetIncludes());
            return entities;

        }

        public async Task<IPagedList<TEntity>> GetAllPaged(GetAllQueryStringParameters<TEntity> request)
        {
            var entities = await repository.FindPagedAsync(q => request.ApplyFilter(q), q => request.ApplySort(q), request.PageNumber - 1, request.PageSize, false, request.GetIncludes());
            return entities;
        }

        public async Task<TEntity> Update(TEntity entity)
        {
            entity = await this.BeforeWrite(entity, ServiceActionType.Update);
            repository.Update(entity);
            await unitOfWork.CommitAsync();
            entity = await this.AfterWrite(entity, ServiceActionType.Update);
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
