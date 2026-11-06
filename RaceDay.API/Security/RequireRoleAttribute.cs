using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Security
{
    /// <summary>
    /// Action filter that requires both an active session AND a specific
    /// role stored in that session. Returns 401 if not logged in, 403 if
    /// logged in as the wrong role. This is what stops Organiser-only
    /// endpoints (e.g. creating events) from being callable by a
    /// Participant, and vice versa.
    /// </summary>
    public class RequireRoleAttribute : ActionFilterAttribute
    {
        private readonly string _requiredRole;

        public RequireRoleAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

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

            var role = context.HttpContext.Session.GetString(SessionKeys.Role);
            if (!string.Equals(role, _requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new
                {
                    message = $"This action requires the '{_requiredRole}' role."
                })
                { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
