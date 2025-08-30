using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly HttpClient _http;
        public DeleteModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        [BindProperty(SupportsGet = true)]
        public int IdUsuario { get; set; }

        public string? UsuarioTexto { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Si tienes rutas espejo, puedes usar /api/Usuario/{IdUsuario}
            var usuario = await _http.GetFromJsonAsync<UsuarioReadDto>($"/api/Usuarios/{IdUsuario}");
            if (usuario is null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                return Page();
            }

            UsuarioTexto = $"{usuario.Nombre} {usuario.Apellido} ({usuario.CorreoUsuario})";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resp = await _http.DeleteAsync($"/api/Usuarios/{IdUsuario}");
            if (resp.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(error) ? "No se pudo eliminar el usuario." : error);

            return await OnGetAsync();
        }

        public class UsuarioReadDto
        {
            public int IdUsuario { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Apellido { get; set; } = string.Empty;
            public string CorreoUsuario { get; set; } = string.Empty;
        }
    }
}
