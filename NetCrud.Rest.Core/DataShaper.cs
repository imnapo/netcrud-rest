using NetCrud.Rest.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetCrud.Rest.Core
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {
        public PropertyInfo[] Properties { get; set; }
        public DataShaper()
        {
            Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }
        public IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> entities, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString, typeof(T));
            return FetchData(entities, requiredProperties);
        }
        public ExpandoObject ShapeData(T entity, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString, typeof(T));
            return FetchDataForEntity(entity, requiredProperties);
        }

        private IList<FieldInfo> GetRequiredProperties(string fieldsString, Type type, IList<FieldInfo> propertyNames = null)
        {
            var requiredProperties = propertyNames ?? new List<FieldInfo>();
            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                var fields = fieldsString.GetFields();
                if (fields.Length == 0) return requiredProperties;
                var Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

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
                        var property = Properties.FirstOrDefault(pi => pi.Name.Equals(fieldName.Trim(), StringComparison.InvariantCultureIgnoreCase));
                        if (property == null)
                            continue;

                        var propName = requiredProperties.FirstOrDefault(x => x.Name == fieldName);
                        var props = GetRequiredProperties(fieldProps, GetInType(property.PropertyType), propName?.Fields);
                        if (propName == null)
                            requiredProperties.Add(new FieldInfo { Name = fieldName, Fields = props, Info = property });
                    }
                }
            }
            return requiredProperties;
        }

        private Type GetInType(Type type)
        {

            if (type.IsGenericType &&

                (type.GetGenericTypeDefinition() == typeof(IList<>)
                || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                || type.GetGenericTypeDefinition() == typeof(List<>)
                )
                )
            {

                var inType = type.GetGenericArguments()[0];
                return inType;
            }
            //type = type.BaseType;

            return type;
        }

        private bool IsGenericList(object obj)
        {
            var type = obj.GetType();
            if (type.IsGenericType &&

                (type.GetGenericTypeDefinition() == typeof(IList<>)
                || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                || type.GetGenericTypeDefinition() == typeof(HashSet<>)
                || type.GetGenericTypeDefinition() == typeof(List<>)
                )
                )
            {

                return true;
            }


            return false;
        }

        private IEnumerable<ExpandoObject> FetchData(IEnumerable<T> entities, IList<FieldInfo> requiredProperties)
        {
            var shapedData = new List<ExpandoObject>();
            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                shapedData.Add(shapedObject);
            }
            return shapedData;
        }

        private IEnumerable<ExpandoObject> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new List<ExpandoObject>();
            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                shapedData.Add(shapedObject);
            }
            return shapedData;
        }
        private ExpandoObject FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedObject = new ExpandoObject();
            foreach (var property in requiredProperties)
            {
                var objectPropertyValue = property.GetValue(entity);
                shapedObject.TryAdd(property.Name, objectPropertyValue);
            }
            return shapedObject;
        }

        private ExpandoObject FetchDataForEntity(object entity, IList<FieldInfo> requiredProperties)
        {
            var shapedObject = new ExpandoObject();
            foreach (var property in requiredProperties)
            {
                var objectPropertyValue = property.Info.GetValue(entity);
                if (property.Fields.Count > 0)
                {
                    bool isList = IsGenericList(objectPropertyValue);
                    if (isList)
                    {
                        dynamic target = objectPropertyValue;
                        var shapedData = new List<ExpandoObject>();
                        for (int i = 0; i < target.Count; i++)
                        {
                            object oo = Enumerable.ElementAt(target, i);
                            var value = FetchDataForEntity(oo, property.Fields);
                            shapedData.Add(value);

                        }
                        shapedObject.TryAdd(property.Name, shapedData);
                    }
                    else
                    {
                        foreach (var p in property.Fields)
                        {
                            var value = FetchDataForEntity(objectPropertyValue, p.Fields);
                            shapedObject.TryAdd(property.Name, value);

                        }
                    }


                }
                else
                {
                    shapedObject.TryAdd(property.Name, objectPropertyValue);

                }

            }
            return shapedObject;
        }

        public IPagedList<ExpandoObject> ShapeData(IPagedList<T> entities, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString, typeof(T));
            var data = FetchData(entities, requiredProperties);

            return new PagedList<ExpandoObject>(data.ToList(), entities.CurrentPage, entities.PageSize, entities.TotalCount);
        }
    }
}
