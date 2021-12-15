using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nv.AspNetCore.Core
{
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public PagedList(List<T> items, int pageNumber, int pageSize, int? count = null)
        {
            TotalCount = count ?? items.Count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            AddRange(items);
        }

        public int CurrentPage { get; }

        public bool HasNextPage => CurrentPage > 0;

        public bool HasPreviousPage => CurrentPage < TotalPages;

        public int PageSize { get; }

        public int TotalCount { get; }

        public int TotalPages { get; }
    }
}
