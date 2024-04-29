using NetCrud.Rest.Core.Extensions;
using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace NetCrud.Rest.Core
{
    public class GetQueryStringParameters<TEntity> where TEntity : class
    {

        public string? Include { get; set; }

        public string? Field { get; set; }

        public virtual IEnumerable<object> ApplyDataShaping(IDataShaper<TEntity> dataShaper, IEnumerable<TEntity> query)
        {
            if (!string.IsNullOrWhiteSpace(Field))
            {
                var shapedData = dataShaper.ShapeData(query, Field);
                return shapedData;
            }
            else return query;
        }

        public virtual object ApplyDataShaping(IDataShaper<TEntity> dataShaper, TEntity entity)
        {
            if (!string.IsNullOrWhiteSpace(Field))
            {
                var shapedData = dataShaper.ShapeData(entity, Field);
                return shapedData;
            }
            else return entity;
        }

        public virtual IPagedList<object> ApplyDataShaping(IDataShaper<TEntity> dataShaper, IPagedList<TEntity> query)
        {
            if (!string.IsNullOrWhiteSpace(Field))
            {
                var shapedData = dataShaper.ShapeData(query, Field);
                return shapedData.Cast();

            }
            else return query.Cast();
        }

        public string[] GetIncludes()
        {
            var includes = IncludeInfo.GetIncludes(Include);
            return IncludeInfo.GetIncludeNames(includes);
        }
    }

    public class GetAllQueryStringParameters<TEntity> : GetQueryStringParameters<TEntity> where TEntity : class
    {

        public bool? Paged { get; set; } = true;

        const int maxPageSize = 500;

        public int? PageNumber { get; set; } = 1;

        private int _pageSize = 20;
        public int? PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value is not null && value < maxPageSize) ? value.Value : maxPageSize;
            }
        }

        public string? Filter { get; set; } = string.Empty;

        public string? Sort { get; set; } = string.Empty ;

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

    public class IncludeInfo
    {
        public static IList<IncludeInfo> GetIncludes(string includesString, IList<IncludeInfo>? includes = null)
        {
            var requiredIncludes = includes ?? new List<IncludeInfo>();
            if (!string.IsNullOrWhiteSpace(includesString))
            {
                var fields = includesString.GetFields();
                if (fields.Length == 0) return requiredIncludes;

                foreach (var field in fields)
                {
                    if (field.Length > 0)
                    {
                        int propStartIndex = -1;
                        int firstDotIndex = field.IndexOf(".");
                        int firstBrackOpenInex = field.IndexOf("[");
                        if (firstBrackOpenInex == -1 && firstDotIndex == -1)
                        {

                        }
                        else if (firstDotIndex != -1)
                        {
                            if (firstBrackOpenInex == -1 || firstDotIndex <= firstBrackOpenInex) propStartIndex = firstDotIndex;
                            else propStartIndex = firstBrackOpenInex;
                        }
                        else
                        {
                            if (!field.EndsWith(']'))
                                throw new Exception("Cant Find ending ']'");
                            propStartIndex = firstBrackOpenInex;
                        }



                        string fieldName = propStartIndex > -1 ? field.Substring(0, propStartIndex) : field;
                        string fieldProps = propStartIndex > -1 ?
                            (propStartIndex == firstDotIndex) ? field.Substring(propStartIndex + 1) : field.Substring(propStartIndex + 1, field.Length - (propStartIndex + 2))
                            : ""
                            ;
                 

                        var propName = requiredIncludes.FirstOrDefault(x => x.PropertyName == fieldName);
                        var props = GetIncludes(fieldProps, propName?.Includes);
                        if (propName == null)
                            requiredIncludes.Add(new IncludeInfo { PropertyName = fieldName, Includes = props });
                    }
                }
            }
            return requiredIncludes;
        }

        public static string[] GetIncludeNames(IList<IncludeInfo> includes)
        {
            List<string> includeStrings = new List<string>();
            foreach (var include in includes)
            {
                includeStrings.AddRange(include.GetIncludeStrings());
            }
            return includeStrings.ToArray();
        }

        public string PropertyName { get; set; }

        public IList<IncludeInfo> Includes { get; set; } = new List<IncludeInfo>();

        public string[] GetIncludeStrings()
        {
            List<string> lst = [];
            if (Includes.Count > 0)
                foreach (var item in Includes)
                {
                    lst.AddRange(item.GetIncludeStrings().Select(x => $"{PropertyName.UppercaseFirst()}.{x}"));
                }
            else lst.Add($"{PropertyName.UppercaseFirst()}");
            return [.. lst];
        }
    }
}
