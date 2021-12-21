using NetCrud.Rest.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace NetCrud.Rest.Core
{
    public class GetQueryStringParameters
    {

        public string Include { get; set; }

        public string[] GetIncludes()
        {
            string[] includes = Include != null ? Include.Split(",") : new string[0];
            includes = includes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.UppercaseFirst()).ToArray();
            return includes;
        }
    }

    public class GetAllQueryStringParameters<TEntity> : GetQueryStringParameters
    {
        public bool Paged { get; set; } = true;

        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 0;

        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        public string Filter { get; set; }

        public string Sort { get; set; }

        public virtual IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query)
        {
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                string deserializedFilter = QueryStringDeserialzer.DeserializeFilter(Filter);

                return query.Where(deserializedFilter);

            }
            else return query;
        }

        public virtual IQueryable<TEntity> ApplySort(IQueryable<TEntity> query)
        {
            if (!string.IsNullOrWhiteSpace(Sort))
            {
                string deserializedSort = QueryStringDeserialzer.DeserializeSort(Sort);
                return query.OrderBy(deserializedSort);
            }
            else return query;
        }
    }
}
