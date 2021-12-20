using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCrud.Rest.Core;
using NetCrud.Rest.Data.Extensions;
using NetCrud.Rest.Models;

namespace NetCrud.Rest.Data
{
    public class Repository<TEntity, TId, TContext> : IRepository<TEntity> where TEntity : EntityBase<TId> where TContext : DbContext
    {
        protected TContext _context = null;
        protected DbSet<TEntity> _table = null;

        public Repository(TContext Context)
        {
            _context = Context;
            _table = _context.Set<TEntity>();
        }
        public async Task<IList<TEntity>> FindAllAsync(params string[] navigationProperties)
        {
            var q = _table.AsQueryable();


            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    q = q.Include(tb);

            return await q.ToListAsync();
        }

        public TEntity FindById(object id, params string[] navigationProperties)
        {
            Type typeParameterType = typeof(TEntity);

            var q = _table.Find(id);
            if (q != null && navigationProperties != null)
                foreach (string tb in navigationProperties)
                {
                    bool isCollection = false;
                    var propType = typeParameterType.GetProperty(tb).PropertyType;

                    if (propType.IsGenericType && propType.GetGenericTypeDefinition()
                            == typeof(ICollection<>))
                    {
                        isCollection = true;
                    }

                    if (!isCollection)
                        foreach (Type interfaceType in propType.GetInterfaces())
                        {
                            if (interfaceType.IsGenericType &&
                                interfaceType.GetGenericTypeDefinition()
                                == typeof(ICollection<>))
                            {
                                isCollection = true;
                                break;
                            }
                        }

                    if (isCollection)
                    {
                        _context.Entry(q).Collection(tb).LoadAsync();
                    }
                    else
                    {
                        _context.Entry(q).Reference(tb).LoadAsync();
                    }
                }

            return q;
        }

        public async Task<TEntity> FindByIdAsync(object id, params string[] navigationProperties)
        {
            Type typeParameterType = typeof(TEntity);

            var q = await _table.FindAsync(id);
            if (q != null && navigationProperties != null)
                foreach (string tb in navigationProperties)
                {
                    bool isCollection = false;
                    var propType = typeParameterType.GetProperty(tb).PropertyType;

                    if (propType.IsGenericType && propType.GetGenericTypeDefinition()
                            == typeof(ICollection<>))
                    {
                        isCollection = true;
                    }

                    if (!isCollection)
                        foreach (Type interfaceType in propType.GetInterfaces())
                        {
                            if (interfaceType.IsGenericType &&
                                interfaceType.GetGenericTypeDefinition()
                                == typeof(ICollection<>))
                            {
                                isCollection = true;
                                break;
                            }
                        }

                    if (isCollection)
                    {
                        await _context.Entry(q).Collection(tb).LoadAsync();
                    }
                    else
                    {
                        await _context.Entry(q).Reference(tb).LoadAsync();
                    }
                }

            return q;
        }

        public async Task AddAsync(TEntity entity)
        {
            setUnchange(entity);
            await _table.AddAsync(entity);
        }


        private void setUnchange(object entity)
        {
            if (entity == null) return;
            //Type typeParameterType = typeof(TEntity);
            Type typeParameterType = entity.GetType();
            var props = typeParameterType.GetProperties();
            foreach (var p in props)
            {
                var type = p.PropertyType;
                if (type.IsClass && typeof(EntityBase).IsAssignableFrom(type))
                {
                    EntityBase value = p.GetValue(entity) as EntityBase;
                    if (value != null && value.Id > 0)
                    {
                        _context.Entry(p.GetValue(entity)).State = EntityState.Unchanged;
                    }
                    setUnchange(p.GetValue(entity));
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition()
                      == typeof(ICollection<>) && typeof(EntityBase).IsAssignableFrom(type.GetGenericArguments()[0]))
                {
                    var values = p.GetValue(entity) as ICollection;

                    //dynamic values = p.GetValue(entity);
                    if (values != null && values.Count > 0)
                    {
                        foreach (var value in values)
                        {
                            var model = value as EntityBase;
                            if (model != null && model.Id > 0)
                                _context.Entry(model).State = EntityState.Unchanged;
                            setUnchange(value);
                        }
                    }
                }

            }
        }

        public void Delete(TEntity entity)
        {
            _table.Remove(entity);
        }

        public async Task<IList<TEntity>> FindAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    query = query.Include(tb);

            query = func != null ? func(query) : query;

            return await query.ToListAsync();
        }

        public IList<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    query = query.Include(tb);

            query = func != null ? func(query) : query;

            return query.ToList();
        }

        public async Task<IList<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, params string[] navigationProperties)
        {
            var q = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    q = q.Include(tb);

            var result = q.Where(predicate);
            return await result.ToListAsync();
        }

        public IList<TEntity> Find(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, params string[] navigationProperties)
        {
            var q = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    q = q.Include(tb);

            var result = q.Where(predicate);
            return result.ToList();
        }

        public async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return await _table.AnyAsync(predicate);
        }

        public bool Any(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return _table.Any(predicate);
        }

        public void Dispose()
        {
            //_context.Dispose();
        }

        public void Update(TEntity model)
        {
            setUnchange(model);
            var entityTracking = _table.Update(model);

        }

        public async Task<IPagedList<TEntity>> FindPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    query = query.Include(tb);

            query = func != null ? func(query) : query;

            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        public async Task<IPagedList<TEntity>> FindPagedAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    query = query.Include(tb);

            query = query.Where(predicate);

            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }
    }

    public class Repository<TEntity, TContext>: Repository<TEntity, int, TContext> where TEntity : EntityBase<int> where TContext : DbContext {
        public Repository(TContext Context) : base(Context)
        {

        }
    }
}
