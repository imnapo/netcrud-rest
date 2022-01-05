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
                    foreach (var item in getLoadRelations(tb))
                        q = q.Include(item);



            return await q.ToListAsync();
        }

        public TEntity FindById(object id, params string[] navigationProperties)
        {
            var q = _table.Find(id);
            if (q != null && navigationProperties != null)
                foreach (string tb in navigationProperties)
                {
                    var relations = getLoadRelations(tb);
                    foreach (var tbItem in relations)
                    {
                        bool parentIsCollection = false;
                        object entity = q;
                        var properties = tbItem.Split(".");

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
                    var relations = getLoadRelations(tb);
                    foreach (var tbItem in relations)
                    {
                        bool parentIsCollection = false;
                        object entity = q;
                        var properties = tbItem.Split(".");
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
                }

            return q;
        }

        public async Task AddAsync(TEntity entity, bool atomic = false)
        {
            if (atomic)
                foreach (var dbEntityEntry in _context.ChangeTracker.Entries())
                {
                    _context.Entry(dbEntityEntry.Entity).State = EntityState.Detached;
                }
            await _table.AddAsync(entity);
            if (atomic) setUnchange(entity);
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

                if (IsInheritFromEntityBase(item.Entity))
                {
                    object value = item.Entity.GetType().GetProperty("Id")?.GetValue(item.Entity);
                    if (value is ValueType)
                    {
                        object obj = Activator.CreateInstance(value.GetType());
                        if (!obj.Equals(value))
                        {
                            _context.Entry(item.Entity).State = EntityState.Unchanged;
                        }
                    }
                    else if (value != null)
                    {
                        _context.Entry(item.Entity).State = EntityState.Unchanged;
                    }

                }

            }
        }

        private void detachAll()
        {

        }

        private void setUnchangeOld(object entity)
        {
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
        private bool IsInheritFromEntityBase(object entity)
        {
            var type2 = entity.GetType();
            while (type2 != null)
            {
                if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(EntityBase<>))
                {
                    return true;
                    //var inType = type2.GetGenericArguments()[0];
                }
                type2 = type2.BaseType;
            }
            return false;
        }

        private string[] getLoadRelations(string propName)
        {
            if (propName.Contains(".")) return new string[] { propName };
            Type typeParameterType = typeof(TEntity);
            var property = typeParameterType.GetProperty(propName);
            if (property != null)
            {
                var attrs = property.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
                if (attrs.ContainsKey(typeof(LoadRelationAttribute).Name))
                {
                    var attr = attrs["LoadRelationAttribute"] as LoadRelationAttribute;
                    return attr.Includes;
                }
                else return new string[] { propName };
            }
            else return new string[] { };

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
                    foreach (var item in getLoadRelations(tb))
                        query = query.Include(item);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;
            return await query.ToListAsync();
        }

        public IList<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    foreach (var item in getLoadRelations(tb))
                        query = query.Include(item);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;

            return query.ToList();
        }

        public async Task<IList<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, params string[] navigationProperties)
        {
            var q = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    foreach (var item in getLoadRelations(tb))
                        q = q.Include(item);

            var result = q.Where(predicate);
            return await result.ToListAsync();
        }

        public IList<TEntity> Find(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, params string[] navigationProperties)
        {
            var q = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    foreach (var item in getLoadRelations(tb))
                        q = q.Include(item);

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
                    foreach (var item in getLoadRelations(tb))
                        query = query.Include(item);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;

            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        public async Task<IPagedList<TEntity>> FindPagedAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    foreach (var item in getLoadRelations(tb))
                        query = query.Include(item);

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
