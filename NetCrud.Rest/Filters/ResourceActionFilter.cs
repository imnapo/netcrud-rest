using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NetCrud.Rest.Core;
using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetCrud.Rest.Filters
{
    public class ResourceActionFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider serviceProvider;
        private readonly string actionName;

        public ResourceActionFilter(IServiceProvider serviceProvider, string actionName = "")
        {
            this.serviceProvider = serviceProvider;
            this.actionName = actionName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.ContainsKey("entity"))
            {
                var entity = context.ActionArguments["entity"];
                Type type = typeof(IResourceActions<>).MakeGenericType(entity.GetType());
                try
                {
                    var service = serviceProvider.GetRequiredService(type);
                    var mth = service.GetType().GetMethod("BeforeCreateAsync");
                    var task = (Task)mth.Invoke(service, new[] { entity });
                    await task.ConfigureAwait(false);

                    var resultProperty = task.GetType().GetProperty("Result");
                    //return resultProperty.GetValue(task);
                }
                catch { }
            }

            var resultContext = await next();
            var myResult = (OkObjectResult)resultContext.Result;
            if (myResult != null)
                try
                {
                    Type type = typeof(IResourceActions<>).MakeGenericType(myResult.Value.GetType());
                    var service = serviceProvider.GetRequiredService(type);
                    var method = service.GetType().GetMethod("AfterCreateAsync");
                    var task = (Task)method.Invoke(service, new[] { myResult.Value });
                    await task.ConfigureAwait(false);

                    var resultProperty = task.GetType().GetProperty("Result");
                }
                catch { }
        }
    }
}
