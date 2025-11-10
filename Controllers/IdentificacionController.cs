using Microsoft.AspNetCore.Mvc;
using Integraciones.Models;
using Integraciones.Services;
using Integraciones;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace WebMvcApp.Controllers
{
    [AuthorizeToken] // Valida token en sesión
    public class IdentificacionController : Controller
    {
        private readonly ApiService _api;

        public IdentificacionController(ApiService api)
        {
            _api = api;
        }

        // Listado de identificiones
        public async Task<IActionResult> Index()
        {
            var identificaciones = await _api.GetIdentificacionesAsync();

            if (identificaciones == null) // Token inválido o expirado
            {
                return RedirectToAction("Login", "Account");
            }

            return View(identificaciones);
        }

        public IActionResult Create()
        {
            LlenarTiposIdentificacion();
            return View(new IdentificacionViewModel());
        }


        // Crear Identificacion (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentificacionViewModel identificacion)
        {
            if (!ModelState.IsValid)
            {
                LlenarTiposIdentificacion();
                return View(identificacion);
            }

            if (!AddTokenCheck()) 
                return RedirectToAction("Login", "Account");

             var (success, message)  = await _api.CreateIdentificacionAsync(identificacion);

            if (success)
            {
              
                ViewBag.MessageType = "success";
                ViewBag.Message = "Identificación creada correctamente.";

                //ModelState.Clear();
                identificacion = new IdentificacionViewModel();
            }
            else
            {
                ViewBag.MessageType = "danger";
                ViewBag.Message = message;
                ModelState.AddModelError("", message);
            }

            //if (success)
            //    return RedirectToAction(nameof(Index));

            //ModelState.AddModelError("", message);

            LlenarTiposIdentificacion();
            return View(identificacion);
        }

       
        private void LlenarTiposIdentificacion()
        {
            ViewBag.TiposIdentificacion = new List<SelectListItem>
        {
        new SelectListItem { Value = "C", Text = "Cédula" },
        new SelectListItem { Value = "R", Text = "RUC" },
        new SelectListItem { Value = "P", Text = "Pasaporte" }
        };
        }
        // Editar Identificacion (GET)
        public async Task<IActionResult> Edit(int id)
        {
            if (!AddTokenCheck()) return RedirectToAction("Login", "Account");

            var identificacion = await _api.GetIdentificacionesAsync(id);
            if (identificacion == null) return NotFound();

            LlenarTiposIdentificacion();
            return View(identificacion);
        }

        // Editar Identificacion (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IdentificacionViewModel identificacion)
        {
            //if (!ModelState.IsValid) return View(identificacion);

            if(!ModelState.IsValid)
    {
                LlenarTiposIdentificacion();
                return View(identificacion);
            }

            if (!AddTokenCheck()) return RedirectToAction("Login", "Account");

            var success = await _api.UpdateIdentificacionAsync(identificacion);
            if (success) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al actualizar la Identificacion");
            ViewBag.TiposIdentificacion = new List<SelectListItem>
            {
                new SelectListItem { Value = "C", Text = "Cédula" },
                new SelectListItem { Value = "R", Text = "RUC" },
                new SelectListItem { Value = "P", Text = "Pasaporte" }
            };
            LlenarTiposIdentificacion();
            return View(identificacion);
        }

        // Inactivar Identificacion (GET)
        public async Task<IActionResult> Inactivar(int id)
        {
            if (!AddTokenCheck()) return RedirectToAction("Login", "Account");

            var identificacion = await _api.GetIdentificacionesAsync(id);
            if (identificacion == null) return NotFound();

            return View(identificacion);
        }

        // Inactivar Identificacion (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inactivar(IdentificacionEstadoViewModel identificacion)
        {
            if (!ModelState.IsValid) return View(identificacion);


            if (!ModelState.IsValid)
            {
                
                var detalle = await _api.GetIdentificacionesAsync(identificacion.idRegistro);
                return View(detalle);
            }

            if (!AddTokenCheck()) return RedirectToAction("Login", "Account");

            var success = await _api.UpdatEstadoAsync(identificacion);
            if (success) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al Inactivar la Identificacion");
            //return View(identificacion);
            var detalleError = await _api.GetIdentificacionesAsync(identificacion.idRegistro);
            return View(detalleError);
        }

        // Eliminar Identificacion (GET)
        public async Task<IActionResult> Delete(int id)
        {
            if (!AddTokenCheck()) return RedirectToAction("Login", "Account");

            var identificacion = await _api.GetIdentificacionesAsync(id);
            if (identificacion == null) return NotFound();

            return View(identificacion);
        }

        // Eliminar identificacion (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!AddTokenCheck()) return RedirectToAction("Login", "Account");

            await _api.DeleteIdentificacionAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // 🔹 Verifica que el token exista y sea válido
        private bool AddTokenCheck()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var exp = HttpContext.Session.GetString("TokenExpiration");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(exp))
                return false;

            if (!DateTime.TryParse(exp, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expTime))
                return false;

            if (expTime <= DateTime.UtcNow)
            {
                HttpContext.Session.Clear();
                return false;
            }

            return true;
        }
    }
}
