using System;
using System.Collections.Generic;
using System.Text;

namespace NetCrud.Rest.Core
{
    public interface IPagedList<T> : IList<T>
    {
        int CurrentPage { get; }
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
    }
}
