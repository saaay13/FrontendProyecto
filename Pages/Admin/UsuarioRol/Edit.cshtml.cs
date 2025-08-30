using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
    {
        private readonly HttpClient _http;
        public EditModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        // Ruta: /Admin/UsuarioRol/Edit/{idUsuario}/{idRolActual}
        [BindProperty(SupportsGet = true)]
        public int IdUsuario { get; set; }

        [BindProperty(SupportsGet = true)]
        public int IdRolActual { get; set; }

        // ---- Inputs que el form editar� ----
        [BindProperty]
        public UsuarioUpdateInput Usuario { get; set; } = new();

        [BindProperty]
        public int RolNuevoId { get; set; }

        // ---- Listas para selects ----
        public List<RolItem> Roles { get; set; } = new();

        public string? TituloUsuario { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // 1) Usuario
            var u = await _http.GetFromJsonAsync<UsuarioDetalleDto>($"/api/Usuarios/{IdUsuario}");
            if (u is null) return NotFound();

            Usuario = new UsuarioUpdateInput
            {
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                CorreoUsuario = u.CorreoUsuario,
                Telefono = u.Telefono,
                Direccion = u.Direccion
            };
            TituloUsuario = $"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})";

            // 2) Roles
            Roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();

            // 3) Pre-selecci�n de rol
            RolNuevoId = IdRolActual;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();
                return Page();
            }

            // 1) Actualizar datos del usuario
            var respUser = await _http.PutAsJsonAsync($"/api/Usuarios/{IdUsuario}", Usuario);
            if (!respUser.IsSuccessStatusCode)
            {
                var error = await respUser.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo actualizar el usuario." : error);
                Roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();
                return Page();
            }

            // 2) Cambiar rol (solo si cambi�)
            if (RolNuevoId != IdRolActual)
            {
                var respRol = await _http.PutAsJsonAsync($"/api/UsuarioRol/{IdUsuario}/{IdRolActual}", RolNuevoId);
                if (!respRol.IsSuccessStatusCode)
                {
                    var error = await respRol.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo actualizar el rol." : error);
                    Roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();
                    return Page();
                }
            }

            return RedirectToPage("Index");
        }

        // ----- DTOs / Inputs locales -----
        public record UsuarioDetalleDto(
            int IdUsuario,
            string Nombre,
            string Apellido,
            string CorreoUsuario,
            string? Telefono,
            string? Direccion,
            DateTime FechaRegistro
        );

        public class UsuarioUpdateInput
        {
            [Required] public string Nombre { get; set; } = string.Empty;
            [Required] public string Apellido { get; set; } = string.Empty;
            [Required, EmailAddress] public string CorreoUsuario { get; set; } = string.Empty;
            [Phone] public string? Telefono { get; set; }
            public string? Direccion { get; set; }
        }

        public record RolItem(int IdRol, string NombreRol);
    }
}
