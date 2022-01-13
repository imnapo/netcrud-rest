using Microsoft.EntityFrameworkCore;
using NetCrud.Rest.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NetCrud.Rest.Data
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> FindByIdAsync(object id, bool forUpdate = false, params string[] navigationProperties);
        TEntity FindById(object id, params string[] navigationProperties);
        Task AddAsync(TEntity model, bool atomic = false);
        void Update(TEntity model, bool attach = true);
        void Delete(TEntity model);
        Task<IList<TEntity>> FindAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, params string[] navigationProperties);

        Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate = null, params string[] navigationProperties);

        IList<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, params string[] navigationProperties);

        IList<TEntity> Find(Expression<Func<TEntity, bool>> predicate, params string[] navigationProperties);

        Task<IPagedList<TEntity>> FindPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> sort = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties);

        Task<IPagedList<TEntity>> FindPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        bool Any(Expression<Func<TEntity, bool>> predicate);

    }
}
