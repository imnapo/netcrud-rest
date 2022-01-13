using System;

namespace NetCrud.Rest.Core
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class EagerLoadAttribute : Attribute
    {
        public EagerLoadAttribute()
        {
        }
    }
}
