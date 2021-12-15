﻿using Microsoft.EntityFrameworkCore;
using Nv.AspNetCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nv.AspNetCore.Data
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<IList<TEntity>> FindAllAsync(params string[] navigationProperties);

        Task<TEntity> FindByIdAsync(object id, params string[] navigationProperties);
        TEntity FindById(object id, params string[] navigationProperties);
        Task AddAsync(TEntity model);
        void Update(TEntity model);
        void Delete(TEntity model);
        Task<IList<TEntity>> FindAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, params string[] navigationProperties);
        IList<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, params string[] navigationProperties);

        Task<IPagedList<TEntity>> FindPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, params string[] navigationProperties);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        bool Any(Expression<Func<TEntity, bool>> predicate);

    }
}
