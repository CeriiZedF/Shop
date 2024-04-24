using Microsoft.AspNetCore.Mvc.Filters;

namespace Shop.Filters
{
    public class CountRequestsAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // узнаём имя контроллера и записываем в session
            string? controllerName = context.RouteData.Values["controller"]?.ToString();
            if (controllerName is not null)
            {
                int? oldValue = context.HttpContext.Session.GetInt32(controllerName);
                context.HttpContext.Session.SetInt32(controllerName, (oldValue ?? 0) + 1);

                string result = $"{controllerName}: {context.HttpContext.Session.GetInt32(controllerName)}";
                Console.WriteLine(result);
            }
            await next();
        }
    }
}
