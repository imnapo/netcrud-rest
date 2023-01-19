using Microsoft.EntityFrameworkCore;
using NetCrud.Rest.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace NetCrud.Rest.EntityFramework.Extensions
{
    public static class IQueryableExtensions
    {
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize, bool getOnlyTotalCount = false) where T : class
        {
            if (source == null)
                return new PagedList<T>(new List<T>(), pageIndex, pageSize);

            //min allowed page size is 1
            pageSize = Math.Max(pageSize, 1);

            var count = await source.CountAsync();

            var data = new List<T>();

            if (!getOnlyTotalCount)
                data.AddRange(await source.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync());

            return new PagedList<T>(data, pageIndex, pageSize, count);
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderby,
            Dictionary<string, PropertyMappingValue> mappingDictioanry)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (mappingDictioanry == null)
            {
                throw new ArgumentNullException("mappingDictionary");
            }

            if (string.IsNullOrWhiteSpace(orderby))
            {
                return source;
            }
            var orderbyAfterSplit = orderby.Split(',');

            foreach (var orderbyClause in orderbyAfterSplit.Reverse())
            {
                var trimmedOrderbyClause = orderbyClause.Trim();
                var orderDesending = trimmedOrderbyClause.EndsWith(" desc");
                var indexOfFirstSpace = trimmedOrderbyClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderbyClause : trimmedOrderbyClause.Remove(indexOfFirstSpace);

                if (!mappingDictioanry.ContainsKey(propertyName))
                {
                    throw new Exception($"Key mapping for {propertyName} is missing");
                }

                var propertyMappingValue = mappingDictioanry[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                    {
                        orderDesending = !orderDesending;
                    }
                    source = source.OrderBy(destinationProperty + (orderDesending ? " descending" : " ascending"));
                }

            }
            return source;
        }

    }
}
