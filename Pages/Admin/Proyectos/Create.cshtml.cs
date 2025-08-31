using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Proyectos
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
            // Si hay errores de validación del formulario, recargo combos y vuelvo a la página
            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync();
                return Page();
            }

            // IMPORTANTE: NO uses slash inicial. Respeta la BaseAddress del HttpClient "API"
            var resp = await _http.PostAsJsonAsync("api/Proyectos", Input);

            if (resp.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var texto = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(texto) ? "No se pudo crear el proyecto." : texto);

            // Si falló el POST, recargo las opciones para que no se pierdan los combos
            await LoadOptionsAsync();
            return Page();
        }

        private async Task LoadOptionsAsync()
        {
            // Siempre SIN slash inicial para que tome BaseAddress
            try
            {
                var ongs = await _http.GetFromJsonAsync<List<OngDto>>("api/Ongs") ?? new();
                ViewData["OngOptions"] = ongs
                    .Select(o => new SelectListItem(o.NombreOng, o.IdOng.ToString()))
                    .ToList();
            }
            catch (Exception ex)
            {
                // No explotes la página si falla, muestra error y deja combos vacíos
                ModelState.AddModelError(string.Empty, $"No se pudieron cargar las ONGs: {ex.Message}");
                ViewData["OngOptions"] = new List<SelectListItem>();
            }

            try
            {
                var usuarios = await _http.GetFromJsonAsync<List<UsuarioDto>>("api/Usuarios") ?? new();
                ViewData["ResponsableOptions"] = usuarios
                    .Select(u => new SelectListItem($"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})", u.IdUsuario.ToString()))
                    .ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"No se pudieron cargar los usuarios: {ex.Message}");
                ViewData["ResponsableOptions"] = new List<SelectListItem>();
            }
        }

        public record OngDto(int IdOng, string NombreOng);
        public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    }
}
