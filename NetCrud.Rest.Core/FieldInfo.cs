using System.Collections.Generic;
using System.Reflection;

namespace NetCrud.Rest.Core
{
    public class FieldInfo
    {
        public string Name { get; set; }

        public PropertyInfo Info { get; set; }

        public IList<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
    }
}
