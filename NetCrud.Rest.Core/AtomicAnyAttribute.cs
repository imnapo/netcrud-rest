using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCrud.Rest.Core
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class AtomicAnyAttribute : NotMappedAttribute
    {
        public string NavProp;

        public AtomicAnyAttribute(string navProperty)
        {
            NavProp = navProperty;
        }
    }
}
