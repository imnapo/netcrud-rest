using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NetCrud.Rest.Core;
using NetCrud.Rest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetCrud.Rest.Filters
{
    public class UpdateResourceActionFilter : IActionFilter
    {
        private readonly IServiceProvider serviceProvider;
        public UpdateResourceActionFilter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.ContainsKey("entity"))
            {   
                var entity = context.ActionArguments["entity"];
                Type type = typeof(IResourceActions<>).MakeGenericType(entity.GetType());
                try
                {
                    var service = serviceProvider.GetRequiredService(type);
                    service.GetType().GetMethod("BeforeUpdate").Invoke(service, new[] { entity });
                }
                catch { }

            }
            else
            {
                //context.Result = new BadRequestObjectResult("Bad id parameter");
                return;
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var myResult = (OkObjectResult)context.Result;
            try
            {
                Type type = typeof(IResourceActions<>).MakeGenericType(myResult.Value.GetType());
                var service = serviceProvider.GetRequiredService(type);
                service.GetType().GetMethod("AfterUpdate").Invoke(service, new[] { myResult.Value });
            }
            catch { }
        }
    }
}
