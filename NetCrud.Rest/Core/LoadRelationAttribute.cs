using System;
using System.Collections.Generic;
using System.Text;

namespace NetCrud.Rest.Core
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class LoadRelationAttribute : Attribute
    {
        public string[] Includes;

        public LoadRelationAttribute(params string[] includes)
        {
            Includes = includes;
        }
    }
}
