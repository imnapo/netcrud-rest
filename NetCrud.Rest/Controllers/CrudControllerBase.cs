using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using NetCrud.Rest.Core;
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
        private readonly IEntityService<TEntity, TId> service;
        private readonly IDataShaper<TEntity> dataShaper;

        public CrudControllerBase(IEntityService<TEntity, TId> service, IDataShaper<TEntity> dataShaper)
        {
            this.service = service;
            this.dataShaper = dataShaper;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllAsync([FromQuery] TParams request)
        {
            if (request.Paged.HasValue && !request.Paged.Value)
            {
                var entities = await service.GetAll(request);
                var shapedData = request.ApplyDataShaping(dataShaper, entities);

                return Ok(shapedData);
            }
            else
            {
                var entities = await service.GetAllPaged(request);
                var shapedData = request.ApplyDataShaping(dataShaper, entities);
                Pageable pageable = new Pageable();
                pageable.LoadPagedList(shapedData);

                //if (mediaType == "application/vnd.Nv.hateoas+json")
                //{
                
                Response.Headers["X-Pagination"] = Newtonsoft.Json.JsonConvert.SerializeObject(pageable);
                //}
                return Ok(shapedData);
            }
        }

        [HttpGet("{id}")]

        public virtual async Task<IActionResult> GetAsync([FromRoute] TId id, [FromQuery] GetQueryStringParameters<TEntity> parameters)
        {
            var entity = await service.Get(id, false, parameters.GetIncludes());

            if (entity == null)
                return NotFound();
            var shapData = parameters.ApplyDataShaping(dataShaper, entity);
            return Ok(shapData);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateAsync(TEntity entity)
        {
            await service.Create(entity);
            return Ok(entity);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> UpdateAsync([FromRoute] TId id, [FromBody] TEntity request)
        {
            if (!request.Id.Equals(id))
                return BadRequest();
            var updatedEntity = await service.Update(request);
            return Ok(updatedEntity);
        }

        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] TId id, [FromBody] JsonPatchDocument<TEntity> request)
        {
            if (request != null)
            {
                var entity = await service.Get(id, true);
                request.ApplyTo(entity, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await service.Update(entity);


                return Ok(entity);

            }
            else
            {
                return BadRequest(ModelState);
            }
        }


        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] TId id)
        {
            var entity = await service.Delete(id);
            if (entity == null)
                return NotFound();
            return NoContent();
        }
    }

    [ApiController]
    public abstract class CrudControllerBase<TEntity> : CrudControllerBase<TEntity, int, GetAllQueryStringParameters<TEntity>> where TEntity : EntityBase<int>
    {
        protected CrudControllerBase(IEntityService<TEntity, int> service, IDataShaper<TEntity> dataShaper) : base(service, dataShaper)
        {
        }
    }

    [ApiController]
    public abstract class CrudControllerBase<TEntity, TId> : CrudControllerBase<TEntity, TId, GetAllQueryStringParameters<TEntity>> where TEntity : EntityBase<TId>
    {
        protected CrudControllerBase(IEntityService<TEntity, TId> service, IDataShaper<TEntity> dataShaper) : base(service, dataShaper)
        {
        }
    }
}
