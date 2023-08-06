
using System.ComponentModel.DataAnnotations.Schema;


namespace NetCrud.Rest.Core
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class LoadRelationAttribute : NotMappedAttribute
    {
        public string[] Includes;

        public LoadRelationAttribute(params string[] includes)
        {
            Includes = includes;
        }
    }

}
