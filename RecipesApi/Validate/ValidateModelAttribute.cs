

using Microsoft.AspNetCore.Mvc.Filters;

namespace RecipesApi.Validate
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
