using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using NetCrud.Rest.Core;
using NetCrud.Rest.Data;
using NetCrud.Rest.Filters;
using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCrud.Rest.Controllers
{
    [ApiController]
    public abstract class CrudControllerBase<TEntity, TId, TParams> : ControllerBase where TEntity : EntityBase<TId> where TParams : GetAllQueryStringParameters<TEntity>
    {
        protected readonly IRepository<TEntity> repository;
        protected readonly IUnitOfWork unitOfWork;

        public CrudControllerBase(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllAsync([FromQuery] TParams request /*[FromHeader(Name = "Accept")] string mediaType*/)
        {
            if (!request.Paged)
            {
                var entities = await repository.FindAsync(q => request.ApplyFilter(q), request.GetIncludes());
                return Ok(entities);
            }
            else
            {
                var entities = await repository.FindPagedAsync(q => request.ApplyFilter(q), request.PageNumber, request.PageSize, false);

                Pageable pageable = new Pageable();
                pageable.LoadPagedList(entities);

                //if (mediaType == "application/vnd.Nv.hateoas+json")
                //{
                Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(pageable));
                //}
                return Ok(entities);
            }
        }

        [HttpGet("{id}")]
        
        public virtual async Task<IActionResult> GetAsync([FromRoute] TId id, [FromQuery] GetQueryStringParameters parameters)
        {
            var entity = await repository.FindByIdAsync(id, parameters.GetIncludes());

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [ServiceFilter(typeof(CreateResourceActionFilter))]
        public virtual async Task<IActionResult> CreateAsync(TEntity entity)
        {
            
            await repository.AddAsync(entity);
            await unitOfWork.CommitAsync();

            return Ok(entity);
            //return CreatedAtAction("Detail", new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(UpdateResourceActionFilter))]
        public virtual async Task<IActionResult> UpdateAsync([FromRoute] TId id, [FromBody] TEntity request)
        {
            if (!request.Id.Equals(id))
                return BadRequest();

            request.ModifiedAt = DateTime.UtcNow;
            repository.Update(request);
            await unitOfWork.CommitAsync();

            return new ObjectResult(request);
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(UpdateResourceActionFilter))]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] TId id, [FromBody] JsonPatchDocument<TEntity> request)
        {
            if (request != null)
            {
                var entity = await repository.FindByIdAsync(id);
                request.ApplyTo(entity, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                entity.ModifiedAt = DateTime.UtcNow;
                await unitOfWork.CommitAsync();

                return new ObjectResult(entity);

            }
            else
            {
                return BadRequest(ModelState);
            }
        }


        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] TId id)
        {
            var entity = await repository.FindByIdAsync(id);

            if (entity == null)
                return NotFound();

            repository.Delete(entity);
            await unitOfWork.CommitAsync();
            return NoContent();
        }
    }

    [ApiController]
    public abstract class CrudControllerBase<TEntity> : CrudControllerBase<TEntity, int, GetAllQueryStringParameters<TEntity>> where TEntity : EntityBase
    {
        protected CrudControllerBase(IRepository<TEntity> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
        }
    }
}
