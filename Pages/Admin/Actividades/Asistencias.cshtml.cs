using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class AsistenciasModel : PageModel
    {
        private readonly HttpClient _http;
        public AsistenciasModel(IHttpClientFactory httpFactory) => _http = httpFactory.CreateClient("API");

        [BindProperty(SupportsGet = true)]
        public int IdActividad { get; set; }

        public string NombreActividad { get; set; } = "";
        public List<InscripcionItem> Inscritos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var act = await _http.GetFromJsonAsync<ActividadDto>($"/api/Actividades/{IdActividad}");
            if (act is null) return NotFound();
            NombreActividad = act.NombreActividad;

            // Inscritos confirmados
            Inscritos = await _http.GetFromJsonAsync<List<InscripcionItem>>
                ($"/api/Inscripciones/por-actividad/{IdActividad}?soloConfirmadas=true") ?? new();
            return Page();
        }

        public async Task<IActionResult> OnPostMarcarAsync(int idInscripcion, bool asistio, string? observacion)
        {
            // Crear registro de asistencia
            var body = new { IdInscripcion = idInscripcion, Asistio = asistio, Observacion = observacion };
            var resp = await _http.PostAsJsonAsync("/api/Asistencias", body);

            TempData[resp.IsSuccessStatusCode ? "ok" : "err"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage(new { IdActividad });
        }

        // DTOs
        public record ActividadDto(int IdActividad, string NombreActividad, DateTime FechaActividad, string? Lugar);
        public class InscripcionItem
        {
            public int IdInscripcion { get; set; }
            public int IdUsuario { get; set; }
            public string NombreUsuario { get; set; } = "";
            public string EstadoInscripcion { get; set; } = "";
        }
    }
}
