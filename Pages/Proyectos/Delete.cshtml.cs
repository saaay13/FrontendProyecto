using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Proyectos
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class DeleteModel : PageModel
    {
        private readonly HttpClient _http;

        public DeleteModel(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("API");
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var p = await _http.GetFromJsonAsync<ProyectoDto>($"/api/Proyectos/{Id}");
            if (p is null) return NotFound();

            Nombre = p.NombreProyecto;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resp = await _http.DeleteAsync($"/api/Proyectos/{Id}");
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var err = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error al eliminar: {err}");
            return Page();
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
    }
}
