using Nv.AspNetCore.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace Nv.AspNetCore.Core
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

        public virtual IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query)
        {
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                string deserializedFilter = QueryStringDeserialzer.Deserialize(Filter);
                return query.Where(deserializedFilter);

            }
            else return query;
        }
    }
}
