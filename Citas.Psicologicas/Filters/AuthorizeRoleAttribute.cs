using Citas.Psicologicas.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Citas.Psicologicas.Filters;

/// <summary>Atributo de autorización basado en roles de sesión</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public AuthorizeRoleAttribute(params string[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.IsAuthenticated())
        {
            context.Result = new RedirectToActionResult(
                "Login", "Auth",
                new { returnUrl = httpContext.Request.Path });
            return;
        }

        if (_roles.Length > 0 && !httpContext.IsInRole(_roles))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
        }
    }
}
