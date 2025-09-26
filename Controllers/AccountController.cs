using Integraciones.Models;
using Integraciones.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Integraciones.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // 🔹 Si ya hay sesión válida → ir directo al Home
            var token = HttpContext.Session.GetString("JwtToken");
            var expiration = HttpContext.Session.GetString("TokenExpiration");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(expiration))
            {
                // Parse en formato ISO 8601
                if (DateTime.TryParse(expiration, null, DateTimeStyles.RoundtripKind, out var expTime)
                    && expTime > DateTime.UtcNow)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Token expirado → limpiar sesión
                    HttpContext.Session.Clear();
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Llamada al API para obtener token
            var (auth, errorMessage) = await _api.LoginAsync(model.Username, model.Password);
            if (auth != null)
            {
                // Guardar token y expiración en Session
                HttpContext.Session.SetString("JwtToken", auth.Token);
                HttpContext.Session.SetString("TokenExpiration", auth.Expiracion.ToString("o")); // ISO 8601

                // Redirigir al Home manualmente
                return RedirectToAction("Index", "Home");
            }

            //ModelState.AddModelError("", "Credenciales inválidas");
            //return View(model);


            // Usar el mensaje que viene del API
            ModelState.AddModelError(string.Empty, errorMessage ?? "Credenciales inválidas");
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Limpiar sesión
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
