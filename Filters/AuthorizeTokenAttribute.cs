using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Globalization;

namespace Integraciones
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeTokenAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            // Excluir Login y Logout del filtro
            var path = context.HttpContext.Request.Path.Value?.ToLower();
            if (path != null && (path.Contains("/account/login") || path.Contains("/account/logout")))
            {
                return; // Permitir acceso
            }

            // Validar token
            var token = session.GetString("JwtToken");
            var expiration = session.GetString("TokenExpiration");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiration))
            {
                // Sin token → redirigir login
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (!DateTime.TryParse(expiration, null, DateTimeStyles.RoundtripKind, out var expTime) || expTime <= DateTime.UtcNow)
            {
                // Token expirado → limpiar sesión y redirigir
                session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Token válido → permite continuar
        }
    }
}
