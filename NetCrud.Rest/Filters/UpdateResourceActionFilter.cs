using System;

namespace NetCrud.Rest.Filters
{
    public class UpdateResourceActionFilter : ResourceActionFilter
    {
        public UpdateResourceActionFilter(IServiceProvider serviceProvider) : base(serviceProvider, "Update")
        {
        }
    }
}
