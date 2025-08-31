using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class DeleteModel : PageModel
    {
        private readonly HttpClient _http;
        public DeleteModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        // Datos mínimos para confirmar
        public string? NombreActividad { get; set; }
        public string? ProyectoNombre { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Inicio { get; set; }
        public TimeSpan Fin { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var dto = await _http.GetFromJsonAsync<ActividadDetalleDto>($"/api/Actividades/{Id}");
            if (dto is null) return NotFound();

            NombreActividad = dto.NombreActividad;
            Fecha = dto.FechaActividad;
            Inicio = dto.HoraInicio;
            Fin = dto.HoraFin;

            // Si tu detalle no trae nombre de proyecto, puedes omitirlo o traerlo aparte
            // Aquí intento obtener el nombre del proyecto:
            var proys = await _http.GetFromJsonAsync<List<ProyectoItem>>("/api/Proyectos") ?? new();
            ProyectoNombre = proys.FirstOrDefault(p => p.IdProyecto == dto.IdProyecto)?.NombreProyecto;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resp = await _http.DeleteAsync($"/api/Actividades/{Id}");
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo eliminar la actividad." : error);
            return await OnGetAsync();
        }
    }
}
