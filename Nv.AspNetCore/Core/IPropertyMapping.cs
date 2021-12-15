using System;
using System.Collections.Generic;
using System.Text;

namespace Nv.AspNetCore.Core
{
    interface IPropertyMapping
    {
    }

    public class PropertyMappingValue
    {
        public IEnumerable<string> DestinationProperties { get; private set; }

        public bool Revert { get; private set; }

        public PropertyMappingValue(IEnumerable<string> destinationProperties,
            bool revert = false)
        {
            DestinationProperties = destinationProperties;
            Revert = revert;
        }
    }

    public class PropertyMapping<TSource, TDesctination> : IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> _mappingDictionary { get; set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            _mappingDictionary = mappingDictionary;
        }
    }
}
