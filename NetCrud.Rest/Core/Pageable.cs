using System;
using System.Collections.Generic;
using System.Text;

namespace NetCrud.Rest.Core
{
    public class Pageable
    {
        #region Methods

        /// <summary>
        /// Ctor
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="pagedList">Entities (models)</param>
        public virtual void LoadPagedList<T>(IPagedList<T> pagedList)
        {
            FirstItem = (pagedList.CurrentPage * pagedList.PageSize) + 1;
            HasNextPage = pagedList.HasNextPage;
            HasPreviousPage = pagedList.HasPreviousPage;
            LastItem = Math.Min(pagedList.TotalCount, ((pagedList.CurrentPage * pagedList.PageSize) + pagedList.PageSize));
            PageNumber = pagedList.CurrentPage + 1;
            PageSize = pagedList.PageSize;
            TotalItems = pagedList.TotalCount;
            TotalPages = pagedList.TotalPages;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The current page index (starts from 0)
        /// </summary>
        public int CurrentPage
        {
            get
            {
                if (PageNumber > 0)
                    return PageNumber - 1;

                return 0;
            }
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public int FirstItem { get; set; }

        public int LastItem { get; set; }

        public bool HasPreviousPage { get; set; }

        public bool HasNextPage { get; set; }

        #endregion
    }
}
