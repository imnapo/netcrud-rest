using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCrud.Rest.Core
{
    public class PagedList<T> : List<T>, IPagedList<T> where T : class
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

        public IPagedList<object> Cast()
        {
            return new PagedList<object>(this.Cast<object>().ToList(), CurrentPage, PageSize, TotalCount);
        }
    }
}
