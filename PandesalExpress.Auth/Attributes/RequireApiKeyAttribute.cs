using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PandesalExpress.Infrastructure.Configs;

namespace PandesalExpress.Auth.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireApiKeyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IOptions<KeysOptions> keysOptions = context.HttpContext
                                                   .RequestServices
                                                   .GetRequiredService<IOptions<KeysOptions>>();

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out StringValues key) ||
            key != keysOptions.Value.AuthKey)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        base.OnActionExecuting(context);
    }
}
