using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Subsy.Web.Filters;

public sealed class FluentValidationModelStateFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        if (executed.Exception is not ValidationException vex || executed.ExceptionHandled)
            return;

        foreach (var error in vex.Errors)
            context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

        context.ActionArguments.TryGetValue("vm", out var model);

        if (context.Controller is Controller controller)
        {
            var actionName = context.RouteData.Values["action"]?.ToString();
            executed.Result = controller.View(actionName, model);
        }
        else
        {
            executed.Result = new BadRequestObjectResult(context.ModelState);
        }

        executed.ExceptionHandled = true;
    }
}
