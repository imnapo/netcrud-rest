using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCrud.Rest.Core;
using NetCrud.Rest.Models;
using System.Reflection;
using NetCrud.Rest.Data;
using NetCrud.Rest.EntityFramework.Extensions;

namespace NetCrud.Rest.EntityFramework
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

        #region Private Methods

        private string[] addEagerLoads(string[] navigationProps, Type type)
        {
            List<string> includes = new List<string>(navigationProps);
            var props = type.GetProperties();
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
                if (attrs.ContainsKey(typeof(EagerLoadAttribute).Name))
                {
                    includes.Add(p.Name);
                }
            }
            return includes.ToArray();
        }

        private IQueryable<TEntity> include(IQueryable<TEntity> query, string[] navigationProperties)
        {
            List<string> includes = new List<string>();

            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                    includes.AddRange(getLoadRelations(tb));
            includes = new List<string>(addEagerLoads(includes.ToArray(), typeof(TEntity)));
            includes.Distinct().ToList().ForEach(x =>
            {
                query = query.Include(x);
            });
            return query;
        }

        private Dictionary<string, string> GetAtomicAnys(string[] navigationProperties)
        {
            Dictionary<string, string> atomics = new Dictionary<string, string>();

            if (navigationProperties != null)
                foreach (string tb in navigationProperties)
                {
                    var result = getAtomicAnys(typeof(TEntity), tb);
                    if (result != null)

                        atomics.Add(tb, result);
                }

            return atomics;
        }

        private Expression<Func<TEntity, bool>> GetFindByIdExpression(object id)
        {
            var propertyName = "Id";
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var predicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(
                    Expression.PropertyOrField(parameter, propertyName),
                    Expression.Constant(id)),
                parameter);
            return predicate;
        }

        private void setUnchangeForAtomic(object entity)
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

                if (IsInheritFromEntityBase(item.Entity.GetType()))
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

        private void setUnchangeDeprached(object entity)
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
                    setUnchangeForAtomic(p.GetValue(entity));
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
                            setUnchangeForAtomic(value);
                        }
                    }
                }

            }
        }
        private bool IsInheritFromEntityBase(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EntityBase<>))
                {
                    return true;
                    //var inType = type2.GetGenericArguments()[0];
                }
                type = type.BaseType;
            }
            return false;
        }

        private string[] getLoadRelations(string propName) => this.getLoadRelations(typeof(TEntity), propName);

        private string[] getLoadRelations(Type parameterType, string propName)
        {
            //if (propName.Contains(".")) return new string[] { propName };
            List<string> lstRelations = new List<string>();

            string[] propNames = propName.Split('.');
            bool isFirst = true;
            Type tp = parameterType;
            foreach (var p in propNames)
            {
                var property = tp.GetProperty(p);
                if (property != null)
                {

                    tp = property.PropertyType;

                    if (tp.IsGenericType && (
                    tp.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    tp.GetGenericTypeDefinition() == typeof(IList<>) ||
                    tp.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                      ))
                    {
                        tp = tp.GetGenericArguments()[0];
                    }

                    var attrs = property.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
                    if (attrs.ContainsKey(typeof(AtomicAnyAttribute).Name))
                    {
                        break;
                    }
                    else if (attrs.ContainsKey(typeof(LoadRelationAttribute).Name))
                    {
                        var attr = attrs[typeof(LoadRelationAttribute).Name] as LoadRelationAttribute;

                        if (isFirst) lstRelations.AddRange(attr.Includes);
                        else lstRelations = lstRelations.SelectMany(x => attr.Includes.Select(a => $"{x}.{a}")).ToList();
                    }
                    else
                    {
                        if (isFirst) lstRelations.Add(p);
                        else lstRelations = lstRelations.Select(x => $"{x}.{p}").ToList();
                    }
                }
                else break;
                isFirst = false;
            }

            return lstRelations.ToArray();
        }

        private string getAtomicAnys(Type parameterType, string propName)
        {
            if (propName.Contains(".")) return null;

            string[] propNames = propName.Split('.');

            Type tp = parameterType;
            var property = tp.GetProperty(propName);
            if (property != null)
            {
                var attrs = property.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
                if (attrs.ContainsKey(typeof(AtomicAnyAttribute).Name))
                {
                    var attr = attrs[typeof(AtomicAnyAttribute).Name] as AtomicAnyAttribute;

                    return attr.NavProp;
                }

            }


            return null;
        }

        private string[] GetAllInclues(Type typeParameterType)
        {
            List<string> inclues = new List<string>();
            var props = typeParameterType.GetProperties();
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
                if (attrs.ContainsKey(typeof(NotMappedAttribute).Name))
                    continue;
                var type = p.PropertyType;
                if (type.IsClass && IsInheritFromEntityBase(type))
                {
                    inclues.Add(p.Name);
                }
                else if (type.IsGenericType && (
                    type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    type.GetGenericTypeDefinition() == typeof(IList<>) ||
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                      ) && IsInheritFromEntityBase(type.GetGenericArguments()[0]))
                {
                    inclues.Add(p.Name);
                }

            }
            return inclues.ToArray();
        }

        private object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        private void SetPropertyValue(object target, string properyName, object value)
        {
            Type type = target.GetType();

            PropertyInfo prop = type.GetProperty(properyName);

            prop.SetValue(target, value, null);
        }

        private async Task ApplyAtomics<T>(T list, string[] navigations) where T : IList<TEntity>
        {
            foreach (var item in list)
            {
                await ApplyAtomics(item, navigations);
            }

        }

        private async Task ApplyAtomics(TEntity item, string[] navigations)
        {
            await ApplyAtomicAnys(item, navigations);
        }

        private async Task<TEntity> ApplyAtomicAnys(TEntity item, string[] navigations)
        {
            var atomics = GetAtomicAnys(navigations);
            foreach (var atomic in atomics)
            {

                var rq = _table.AsQueryable();
                var res = await rq.Where($"x => x.Id == {item.Id} && x.{atomic.Value}.Any()").AnyAsync();
                SetPropertyValue(item, atomic.Key, res);
            }
            return item;
        }

        #endregion

        public TEntity FindById(object id, bool includeAll = false, params string[] navigationProperties)
        {
            if (includeAll)
            {
                navigationProperties = GetAllInclues(typeof(TEntity));
            }

            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties.Distinct().ToArray());

            return query.FirstOrDefault(GetFindByIdExpression(id));

        }

        public async Task<TEntity> FindByIdAsync(object id, bool includeAll = false, params string[] navigationProperties)
        {
            if (includeAll)
            {
                navigationProperties = GetAllInclues(typeof(TEntity));
            }

            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties);

            var entity = await query.FirstOrDefaultAsync(GetFindByIdExpression(id));
            //if (includeAll)
            //{
            //    var trackedEntities = _context.ChangeTracker.Entries().ToList();

            //    foreach (var item in trackedEntities)
            //    {
            //        var a = item.Entity;
            //        if (entity != a)
            //            item.State = EntityState.Detached;
            //    }
            //}

            await ApplyAtomics(entity, navigationProperties);
            return entity;
        }

        public async Task AddAsync(TEntity entity, bool atomic = false)
        {
            if (atomic)
                foreach (var dbEntityEntry in _context.ChangeTracker.Entries())
                {
                    _context.Entry(dbEntityEntry.Entity).State = EntityState.Detached;
                }
            await _table.AddAsync(entity);
            if (atomic) setUnchangeForAtomic(entity);
        }

        public void Delete(TEntity entity)
        {
            _table.Remove(entity);


        }

        public async Task<IList<TEntity>> FindAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;
            var result = await query.ToListAsync();
            await ApplyAtomics(result, navigationProperties);
            return result;
        }

        public IList<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;

            return query.ToList();
        }

        public async Task<IList<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate = null, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties);

            var result = predicate != null ? query.Where(predicate) : query;
            var data = await result.ToListAsync();
            await ApplyAtomics(data, navigationProperties);
            return data;
        }

        public IList<TEntity> Find(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties);

            var result = query.Where(predicate);
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
            setUnchangeForAtomic(model);
            if (attach)
                _table.Update(model);

        }

        public async Task<IPagedList<TEntity>> FindPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties);

            query = func != null ? func(query) : query;
            query = sort != null ? sort(query) : query;

            var result = await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
            await ApplyAtomics(result, navigationProperties);
            return result;
        }

        public async Task<IPagedList<TEntity>> FindPagedAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties)
        {
            var query = _table.AsQueryable();
            query = this.include(query, navigationProperties);

            query = query.Where(predicate);

            var result = await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
            await ApplyAtomics(result, navigationProperties);
            return result;
        }

        public void Detach(object model)
        {
            var entry = _context.Entry(model);
            if (entry != null)
                entry.State = EntityState.Detached;
        }

        public async Task ReloadAsync(object model)
        {
            var entry = _context.Entry(model);
            if (entry != null)
                await entry.ReloadAsync();
        }

        public Dictionary<string, object> GetChangedProperties(TEntity model)
        {
            var entry = _context.Entry(model);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            // Gets all properties from the changed entity by reflection.
            foreach (var entityProperty in entry.Properties)
            {           
                if (entityProperty.IsModified)
                    properties.Add(entityProperty.Metadata.Name, entityProperty.OriginalValue);             
            }

            return properties;
        }
    }

    public class Repository<TEntity, TContext> : Repository<TEntity, int, TContext> where TEntity : EntityBase<int> where TContext : DbContext
    {
        public Repository(TContext Context) : base(Context)
        {

        }
    }
}
