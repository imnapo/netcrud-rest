using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace NetCrud.Rest.Core
{
    public interface IDataShaper<T> where T : class
    {
        IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> entities, string fieldsString);

        IPagedList<ExpandoObject> ShapeData(IPagedList<T> entities, string fieldsString);

        ExpandoObject ShapeData(T entity, string fieldsString);
    }
}
