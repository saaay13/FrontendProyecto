using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    [Authorize(Roles = "Administrador")]
    public class CreateModel : PageModel
    {
        private readonly HttpClient _http;
        public CreateModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        // Solo datos del usuario (el rol lo asigna el backend automáticamente)
        [BindProperty]
        public RegisterInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var payload = new RegisterRequest
            {
                Nombre = Input.Nombre,
                Apellido = Input.Apellido,
                CorreoUsuario = Input.CorreoUsuario,
                Password = Input.Password,
                Telefono = Input.Telefono
                // Si tu RegisterRequest tuviera Dirección, agrégala aquí.
            };

            // Importante: la ruta correcta de tu backend es /api/Auth/register
            var resp = await _http.PostAsJsonAsync("/api/Auth/register", payload);

            if (resp.IsSuccessStatusCode)
                return RedirectToPage("Index");

            // Intenta leer un { mensaje = "..."} desde el backend
            var body = await resp.Content.ReadAsStringAsync();
            try
            {
                var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("mensaje", out var m))
                {
                    ModelState.AddModelError(string.Empty, m.GetString() ?? "Error al registrar.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(body) ? "No se pudo registrar el usuario." : body);
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(body) ? "No se pudo registrar el usuario." : body);
            }

            return Page();
        }

        // ===== Inputs/DTOs =====

        public class RegisterInput
        {
            [Required] public string Nombre { get; set; } = string.Empty;
            [Required] public string Apellido { get; set; } = string.Empty;
            [Required, EmailAddress] public string CorreoUsuario { get; set; } = string.Empty;
            [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
            public string? Telefono { get; set; }
        }

        // Debe coincidir con tu RegisterRequest del backend
        public class RegisterRequest
        {
            public string Nombre { get; set; } = string.Empty;
            public string Apellido { get; set; } = string.Empty;
            public string CorreoUsuario { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? Telefono { get; set; }
        }
    }
}
