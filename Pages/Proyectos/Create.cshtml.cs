using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Proyectos
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class CreateModel : PageModel
    {
        private readonly HttpClient _http;
        public CreateModel(IHttpClientFactory httpFactory) => _http = httpFactory.CreateClient("API");

        [BindProperty] public ProyectoInput Input { get; set; } = new();

        public async Task OnGetAsync() => await LoadOptionsAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadOptionsAsync();           
            if (!ModelState.IsValid) return Page();

            var resp = await _http.PostAsJsonAsync("/api/Proyectos", Input);
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            ModelState.AddModelError(string.Empty, $"Error al crear: {await resp.Content.ReadAsStringAsync()}");
            return Page();
        }

        private async Task LoadOptionsAsync()
        {
            var ongs = await _http.GetFromJsonAsync<List<OngDto>>("/api/Ongs") ?? new();
            ViewData["OngOptions"] = ongs.Select(o => new SelectListItem(o.NombreOng, o.IdOng.ToString())).ToList();

            var usuarios = await _http.GetFromJsonAsync<List<UsuarioDto>>("/api/Usuarios") ?? new();
            ViewData["ResponsableOptions"] = usuarios
                .Select(u => new SelectListItem($"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})", u.IdUsuario.ToString()))
                .ToList();
        }

        public record OngDto(int IdOng, string NombreOng);
        public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    }
}
