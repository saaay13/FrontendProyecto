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

        [BindProperty(SupportsGet = true)]
        public int IdRol { get; set; }

        public string? UsuarioTexto { get; set; }
        public string? RolTexto { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuarios = await _http.GetFromJsonAsync<List<UsuarioItem>>("/api/Auth") ?? new();
            var roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();

            var u = usuarios.FirstOrDefault(x => x.IdUsuario == IdUsuario);
            var r = roles.FirstOrDefault(x => x.IdRol == IdRol);

            UsuarioTexto = u is null ? IdUsuario.ToString() : $"{u.Nombre} ({u.CorreoUsuario})";
            RolTexto = r is null ? IdRol.ToString() : r.NombreRol;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resp = await _http.DeleteAsync($"/api/UsuarioRol/{IdUsuario}/{IdRol}");
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo eliminar la asignación." : error);
            return await OnGetAsync();
        }
    }
}
