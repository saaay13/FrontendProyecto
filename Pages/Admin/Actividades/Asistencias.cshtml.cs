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

            Inscritos = await _http.GetFromJsonAsync<List<InscripcionItem>>
                ($"/api/Inscripciones/por-actividad/{IdActividad}?soloConfirmadas=true") ?? new();

            // Traer estado de asistencia por inscripción
            var tareas = Inscritos.Select(async i =>
            {
                var estado = await _http.GetFromJsonAsync<EstadoAsistenciaDto>($"/api/Asistencias/estado/{i.IdInscripcion}");
                i.EstadoAsistencia = estado;

                // Deshabilitar botón si hoy no está en rango o si ya registró hoy
                var hoy = DateTime.Today;
                var enRango = estado != null && hoy >= estado.Rango.Inicio.Date && hoy <= estado.Rango.Fin.Date;

                var yaMarcoHoy = estado?.DiasRegistrados.Any(d => d.Date == hoy) == true;

                i.BotonDeshabilitadoHoy = !enRango || yaMarcoHoy;
            });

            await Task.WhenAll(tareas);

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

            // UI helpers (no vienen del backend de inscripciones)
            public EstadoAsistenciaDto? EstadoAsistencia { get; set; }
            public bool BotonDeshabilitadoHoy { get; set; }
        }

        public class EstadoAsistenciaDto
        {
            public RangoDto Rango { get; set; } = new();
            public List<DateTime> DiasRegistrados { get; set; } = new();
            public List<DateTime> DiasPendientes { get; set; } = new();
            public bool Completado { get; set; }
        }
        public class RangoDto
        {
            public DateTime Inicio { get; set; }
            public DateTime Fin { get; set; }
            public int DiasRequeridos { get; set; }
        }

    }
}
