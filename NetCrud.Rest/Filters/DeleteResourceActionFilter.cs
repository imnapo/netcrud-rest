using System;

namespace NetCrud.Rest.Filters
{
    public class DeleteResourceActionFilter : ResourceActionFilter
    {
        public DeleteResourceActionFilter(IServiceProvider serviceProvider) : base(serviceProvider, "Delete")
        {
        }
    }
}
