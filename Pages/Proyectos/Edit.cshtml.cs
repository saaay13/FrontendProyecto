using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Proyectos
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class EditModel : PageModel
    {
        private readonly HttpClient _http;

        public EditModel(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("API");
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public ProyectoInput Input { get; set; } = new();

        public List<SelectListItem> OngOptions { get; private set; } = new();
        public List<SelectListItem> ResponsableOptions { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadOptionsAsync();

            var p = await _http.GetFromJsonAsync<ProyectoDto>($"/api/Proyectos/{Id}");
            if (p is null) return NotFound();

            Enum.TryParse<EstadoProyectoEnum>(p.EstadoProyecto, out var estado);

            Input = new ProyectoInput
            {
                IdOng = p.IdOng,
                NombreProyecto = p.NombreProyecto,
                Descripcion = p.Descripcion,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                EstadoProyecto = estado,
                IdResponsable = p.IdResponsable
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadOptionsAsync();
            if (!ModelState.IsValid) return Page();

            // Si tu API exige Id en el body que coincida con la URL:
            var body = new
            {
                IdProyecto = Id,
                Input.IdOng,
                Input.NombreProyecto,
                Input.Descripcion,
                Input.FechaInicio,
                Input.FechaFin,
                EstadoProyecto = Input.EstadoProyecto, 
                Input.IdResponsable
            };

            var resp = await _http.PutAsJsonAsync($"/api/Proyectos/{Id}", body);
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error al actualizar: {error}");
            return Page();
        }

        private async Task LoadOptionsAsync()
        {
            var ongs = await _http.GetFromJsonAsync<List<OngDto>>("/api/Ongs") ?? new();
            OngOptions = ongs.Select(o => new SelectListItem(o.NombreOng, o.IdOng.ToString())).ToList();
            ViewData["OngOptions"] = OngOptions;

            var usuarios = await _http.GetFromJsonAsync<List<UsuarioDto>>("/api/Auth") ?? new();
            ResponsableOptions = usuarios.Select(u => new SelectListItem($"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})", u.IdUsuario.ToString()))
                                         .ToList();
            ViewData["ResponsableOptions"] = ResponsableOptions;
        }

        public record ProyectoDto(
            int IdProyecto,
            int IdOng,
            string NombreProyecto,
            string Descripcion,
            DateTime FechaInicio,
            DateTime FechaFin,
            string EstadoProyecto,
            int IdResponsable
        );

        public record OngDto(int IdOng, string NombreOng);
        public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    }
}
