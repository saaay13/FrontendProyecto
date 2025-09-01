using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
    {
        private readonly HttpClient _http;
        public EditModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        
        [BindProperty(SupportsGet = true)]
        public int IdUsuario { get; set; }

        [BindProperty(SupportsGet = true)]
        public int IdRolActual { get; set; }

        // ---- Inputs 
        [BindProperty]
        public UsuarioUpdateInput Usuario { get; set; } = new();

        [BindProperty]
        public int RolNuevoId { get; set; }

        // ---- Listas para selects ----
        public List<RolItem> Roles { get; set; } = new();

        public string? TituloUsuario { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
       
            var u = await _http.GetFromJsonAsync<UsuarioDetalleDto>($"/api/Usuarios/{IdUsuario}");
            if (u is null) return NotFound();

            Usuario = new UsuarioUpdateInput
            {
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                CorreoUsuario = u.CorreoUsuario,
                Telefono = u.Telefono,
            };
            TituloUsuario = $"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})";

         
            Roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();

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

          
            var respUser = await _http.PutAsJsonAsync($"/api/Usuarios/{IdUsuario}", Usuario);
            if (!respUser.IsSuccessStatusCode)
            {
                var error = await respUser.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo actualizar el usuario." : error);
                Roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();
                return Page();
            }

            
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

        // ----- DTOs / Inputs 
        public record UsuarioDetalleDto(
            int IdUsuario,
            string Nombre,
            string Apellido,
            string CorreoUsuario,
            string? Telefono,
            DateTime FechaRegistro
        );

        public class UsuarioUpdateInput
        {
            [Required] public string Nombre { get; set; } = string.Empty;
            [Required] public string Apellido { get; set; } = string.Empty;
            [Required, EmailAddress] public string CorreoUsuario { get; set; } = string.Empty;
            [Phone] public string? Telefono { get; set; }
        }

        public record RolItem(int IdRol, string NombreRol);
    }
}
