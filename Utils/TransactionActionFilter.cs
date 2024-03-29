﻿using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Utils
{
    public sealed class TransactionActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
            {
                return;
            }

            var controllerUoWAttrs = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<UoWAttribute>();
            var actionUoWAttrs = controllerActionDescriptor.MethodInfo.GetCustomAttribute<UoWAttribute>();
            if (controllerUoWAttrs != null || actionUoWAttrs != null)
            {
                using var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                ActionExecutedContext result = await next();
                if (result.Exception == null || result.ExceptionHandled)
                {
                    ts.Complete();
                }
                else
                {
                    ts.Dispose();
                }
            }
            else
            {
                await next();
            }
        }
    }
}