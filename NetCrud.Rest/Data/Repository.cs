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
            var q = _table.Find(id);
            if (q != null && navigationProperties != null)
                foreach (string tb in navigationProperties)
                {
                    bool parentIsCollection = false;
                    object entity = q;
                    var properties = tb.Split(".");
                    foreach (var property in properties)
                    {
                        if (entity == null) break;

                        if (parentIsCollection)
                        {
                            var v = (IEnumerable)entity;
                            foreach (var item in v)
                            {
                                _context.Entry(item).Navigation(property).Load();
                            }
                            break;
                        }
                        else
                        {
                            _context.Entry(entity).Navigation(property).LoadAsync();
                        }

                        Type typeParameterType = entity.GetType().GetProperty(property).PropertyType;

                        if (typeParameterType.IsGenericType && ((typeParameterType.GetGenericTypeDefinition()
                            == typeof(HashSet<>)) || (typeParameterType.GetGenericTypeDefinition()
                            == typeof(ICollection<>)) || (typeParameterType.GetGenericTypeDefinition()
                            == typeof(IList<>))))
                        {
                            parentIsCollection = true;
                        }
                        else parentIsCollection = false;

                        entity = entity.GetType().GetProperty(property).GetValue(entity);
                    }
                }

            return q;
        }

        public async Task<TEntity> FindByIdAsync(object id, bool forUpdate = false, params string[] navigationProperties)
        {
            var q = await _table.FindAsync(id);
            if (forUpdate)
            {
                navigationProperties = GetAllInclues(q);
            }

            if (q != null && navigationProperties != null)
                foreach (string tb in navigationProperties)
                {
                    bool parentIsCollection = false;
                    object entity = q;
                    var properties = tb.Split(".");
                    foreach (var property in properties)
                    {
                        if (entity == null) break;

                        if (parentIsCollection)
                        {
                            var v = (IEnumerable)entity;
                            foreach (var item in v)
                            {
                                await _context.Entry(item).Navigation(property).LoadAsync();
                            }
                            break;
                        }
                        else
                        {
                            await _context.Entry(entity).Navigation(property).LoadAsync();
                        }

                        Type typeParameterType = entity.GetType().GetProperty(property).PropertyType;

                        if (typeParameterType.IsGenericType && ((typeParameterType.GetGenericTypeDefinition()
                            == typeof(HashSet<>)) || (typeParameterType.GetGenericTypeDefinition()
                            == typeof(ICollection<>)) || (typeParameterType.GetGenericTypeDefinition()
                            == typeof(IList<>))))
                        {
                            parentIsCollection = true;
                        }
                        else parentIsCollection = false;

                        entity = entity.GetType().GetProperty(property).GetValue(entity);
                    }
                }

            return q;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _table.AddAsync(entity);
            setUnchange(entity);
        }

        private void setUnchange(object entity)
        {
            var modifiedEntities = _context.ChangeTracker.Entries()
            .Where(p => p.State == EntityState.Modified).ToList();

            foreach (var item in modifiedEntities)
            {
                var a = item.Entity;
                if (entity != a)
                    item.State = EntityState.Unchanged;
            }

            modifiedEntities = _context.ChangeTracker.Entries()
            .Where(p => p.State == EntityState.Added).ToList();

            foreach (var item in modifiedEntities)
            {
                var a = item.Entity as EntityBase;
                if (a != null && a.Id > 0)
                    item.State = EntityState.Unchanged;
            }

            return;


            var mems = _context.Entry(entity).Members.ToList();


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
                else if (type.IsGenericType && (
                    type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    type.GetGenericTypeDefinition() == typeof(IList<>) ||
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                      ) && typeof(EntityBase).IsAssignableFrom(type.GetGenericArguments()[0]))
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

        private string[] GetAllInclues(object entity)
        {
            List<string> inclues = new List<string>();
            Type typeParameterType = entity.GetType();
            var props = typeParameterType.GetProperties();
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
                if (attrs.ContainsKey("NotMappedAttribute"))
                    continue;
                var type = p.PropertyType;
                if (type.IsClass && typeof(EntityBase).IsAssignableFrom(type))
                {
                    inclues.Add(p.Name);
                }
                else if (type.IsGenericType && (
                    type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    type.GetGenericTypeDefinition() == typeof(IList<>) ||
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                      ) && typeof(EntityBase).IsAssignableFrom(type.GetGenericArguments()[0]))
                {
                    inclues.Add(p.Name);
                }

            }
            return inclues.ToArray();
        }

        public void Delete(TEntity entity)
        {
            _table.Remove(entity);
        }

        public async Task<IList<TEntity>> FindAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    query = query.Include(tb);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;
            return await query.ToListAsync();
        }

        public IList<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    query = query.Include(tb);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;

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

        public void Update(TEntity model, bool attach = true)
        {
            setUnchange(model);
            if (attach)
                _table.Update(model);

        }

        public async Task<IPagedList<TEntity>> FindPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    query = query.Include(tb);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;

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

    public class Repository<TEntity, TContext> : Repository<TEntity, int, TContext> where TEntity : EntityBase<int> where TContext : DbContext
    {
        public Repository(TContext Context) : base(Context)
        {

        }
    }
}
