using System;

namespace NetCrud.Rest.Filters
{
    public class CreateResourceActionFilter : ResourceActionFilter
    {
        public CreateResourceActionFilter(IServiceProvider serviceProvider) : base(serviceProvider, "Create")
        {
        }
    }
}
