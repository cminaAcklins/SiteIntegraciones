using System.Net.Http;
using System.Net.Http.Json;
using Integraciones.Models;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Integraciones.Application.DTOs;

namespace Integraciones.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _http = httpClient;
            _httpContextAccessor = httpContextAccessor;
            //_http.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);
        }

        /// <summary>
        /// Agrega el token al header del request si está vigente
        /// </summary>
        private bool AddTokenHeader()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return false;

            var token = session.GetString("JwtToken");
            var expiration = session.GetString("TokenExpiration");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiration))
                return false;

            if (!DateTime.TryParse(expiration, null, DateTimeStyles.RoundtripKind, out var expTime) || expTime <= DateTime.UtcNow)
            {
                // Token expirado → limpiar sesión
                session.Clear();
                return false;
            }

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return true;
        }

        /// <summary>
        /// Login: espera un AuthResponse con Token + Expiración
        /// </summary>
 
        public async Task<(AuthResponse? Auth, string? ErrorMessage)> LoginAsync(string username, string password)
        {
            var resp = await _http.PostAsJsonAsync("auth/login", new { username, password });
            if (resp.IsSuccessStatusCode)
            {
                var authResponse = await resp.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    // Persistir token en sesión
                    var session = _httpContextAccessor.HttpContext?.Session;
                    session?.SetString("JwtToken", authResponse.Token);
                    session?.SetString("TokenExpiration", authResponse.Expiracion.ToString("o")); // ISO 8601
                    session?.SetString("Usuario", username);
                }
                return (authResponse, null);
            }

            // Leer mensaje de error devuelto por la API
            var error = await resp.Content.ReadFromJsonAsync<ErrorDto>();
            return (null, error?.Mensaje);


        }

        public async Task<List<IdentificacionViewModel>?> GetIdentificacionesAsync()
        {
            if (!AddTokenHeader()) return null;

            var resp = await _http.GetAsync("Identificacion/ListaIdentificaciones"); 
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<List<IdentificacionViewModel>>();
        }

        public async Task<IdentificacionViewModel?> GetIdentificacionesAsync(int id)
        {
            if (!AddTokenHeader()) return null;

            // Llamada al endpoint real de consulta de la API
            var resp = await _http.GetAsync($"Identificacion/ConsultIdentificacion/{id}");
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<IdentificacionViewModel>();
        }


        public async Task<bool> CreateIdentificacionAsync(IdentificacionViewModel identificacion)
        {
            if (!AddTokenHeader()) return false;

            // Mapear ViewModel -> DTO

            var usuario = _httpContextAccessor.HttpContext?.Session.GetString("Usuario");


            var dto = new IdentificacionDto
            {
                idRegistro = identificacion.idRegistro,
                txtIdentificacion = identificacion.txtIdentificacion,
                txtTipoIdentificacion = identificacion.txtTipoIdentificacion,
                txtNombresApellidos = identificacion.txtNombresApellidos,
                txtFechaIngreso = DateTime.Now,
                txtUsuarioIngreso = usuario,
                txtestado = identificacion.txtestado,

            };

            // Llamada al endpoint real de creación de la API
            var resp = await _http.PostAsJsonAsync("Identificacion/CrearIdentificacion", dto);

            return resp.IsSuccessStatusCode;
        }


        public async Task<bool> UpdateIdentificacionAsync(IdentificacionViewModel identificacion)
        {


            var usuario = _httpContextAccessor.HttpContext?.Session ?.GetString("Usuario");

            if (!AddTokenHeader()) return false;

            var dto = new IdentificacionDto
            {
                idRegistro = identificacion.idRegistro,
                txtIdentificacion = identificacion.txtIdentificacion,
                txtTipoIdentificacion = identificacion.txtTipoIdentificacion,
                txtNombresApellidos = identificacion.txtNombresApellidos,
                //txtUsuarioIngreso = usuario ?? string.Empty,
                txtestado = identificacion.txtestado,

            };

            var resp = await _http.PutAsJsonAsync($"Identificacion/ActualizaIdentificacion/{identificacion.idRegistro}", dto);

            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatEstadoAsync(IdentificacionEstadoViewModel identificacion)
        {


            if (!AddTokenHeader()) return false;

            var dto = new IdentificacionEstadoDto
            {
               
                txtestado = identificacion.txtestado,

            };

            var resp = await _http.PutAsJsonAsync($"Identificacion/ActualizaEstado/{identificacion.idRegistro}", dto);

            return resp.IsSuccessStatusCode;
        }


        public async Task<bool> DeleteIdentificacionAsync(int id)
        {
            if (!AddTokenHeader()) return false;

            // Llamada al endpoint real de eliminación de la API
            var resp = await _http.DeleteAsync($"Identificacion/EliminaIdentificacion/{id}");

            return resp.IsSuccessStatusCode;
        }




    }
}
