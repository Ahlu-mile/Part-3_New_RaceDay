using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Security
{
    /// <summary>
    /// Action filter that rejects the request with 401 Unauthorized unless
    /// the caller has an active session (i.e. has logged in). Session state
    /// is written by AuthController.Login and read here on every subsequent
    /// request, fulfilling the "session management to maintain the user's
    /// authenticated state" requirement.
    /// </summary>
    public class RequireAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32(SessionKeys.UserId);

            if (userId is null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "You must be logged in to access this resource."
                });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
