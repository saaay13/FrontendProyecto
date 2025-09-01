using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Reconocimientos
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class EmitirModel : PageModel
    {
        private readonly HttpClient _http;

        public EmitirModel(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("API"); // Asegúrate que apunte al puerto del backend
        }

        [BindProperty(SupportsGet = true)]
        public int? UsuarioId { get; set; }

        public List<SelectListItem> UsuariosOptions { get; set; } = new();
        public List<SelectListItem> OngOptions { get; set; } = new();
        public List<InscripcionVm> InscripcionesVM { get; set; } = new();

        public bool PuedeEmitirCarnet { get; set; }
        public string? MotivoBloqueoCarnet { get; set; }

        public async Task OnGetAsync()
        {
            await CargarUsuariosAsync();
            await CargarOngsAsync();

            if (UsuarioId is null || UsuarioId <= 0) return;

            // 1) Traer inscripciones del usuario
            var inscripciones = await BuscarInscripcionesDeUsuario(UsuarioId.Value);

            // 2) Calcular estado de asistencia por inscripción (usando /api/Asistencias/estado/{id})
            var tareas = inscripciones.Select(async i =>
            {
                var estado = await BuscarEstadoAsistencia(i.IdInscripcion);

                // Seguridad por si el backend aún no tiene el endpoint o no encuentra datos
                int diasRequeridos = estado?.Rango.DiasRequeridos ?? 0;
                int diasRegistrados = estado?.DiasRegistrados?.Count ?? 0;
                bool completado = estado?.Completado ?? false;

                // Elegibilidad de certificado:
                // - Inscripción confirmada
                // - La actividad ya ocurrió (hoy >= fecha actividad)
                // - Estado.Completado == true
                // - No tiene certificado emitido aún
                bool elegibleCert = i.EstadoInscripcion == "Confirmada"
                                    && DateTime.Today >= i.FechaActividad.Date
                                    && completado
                                    && !i.YaTieneCertificado;

                InscripcionesVM.Add(new InscripcionVm
                {
                    IdInscripcion = i.IdInscripcion,
                    IdActividad = i.IdActividad,
                    NombreActividad = i.NombreActividad,
                    FechaActividad = i.FechaActividad,
                    Lugar = i.Lugar,
                    EstadoInscripcion = i.EstadoInscripcion,
                    DiasRequeridos = diasRequeridos,
                    DiasRegistrados = diasRegistrados,
                    Completado = completado,
                    ElegibleCertificado = elegibleCert
                });
            });

            await Task.WhenAll(tareas);

            // 3) Elegibilidad carnet simplificada (backend valida igual)
            if (inscripciones.Any(x => x.EstadoInscripcion == "Confirmada"))
            {
                PuedeEmitirCarnet = true;
                MotivoBloqueoCarnet = null;
            }
            else
            {
                PuedeEmitirCarnet = false;
                MotivoBloqueoCarnet = "El usuario no tiene inscripciones confirmadas.";
            }
        }

        public async Task<IActionResult> OnPostEmitirCertificadoAsync(int IdActividad)
        {
            if (UsuarioId is null) return BadRequest("Usuario no seleccionado.");

            var body = new { IdUsuario = UsuarioId.Value, IdActividad };
            var resp = await _http.PostAsJsonAsync("/api/Certificados", body);

            TempData[resp.IsSuccessStatusCode ? "ok" : "err"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage(new { UsuarioId });
        }

        public async Task<IActionResult> OnPostEmitirCarnetAsync(int IdOng)
        {
            if (UsuarioId is null) return BadRequest("Usuario no seleccionado.");

            var body = new { IdUsuario = UsuarioId.Value, IdOng };
            var resp = await _http.PostAsJsonAsync("/api/Carnets", body);

            TempData[resp.IsSuccessStatusCode ? "ok" : "err"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage(new { UsuarioId });
        }

        // ---------- Helpers ----------
        private async Task CargarUsuariosAsync()
        {
            var usuarios = await _http.GetFromJsonAsync<List<UsuarioDto>>("/api/Usuarios") ?? new();
            UsuariosOptions = usuarios
                .Select(u => new SelectListItem($"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})", u.IdUsuario.ToString()))
                .ToList();
        }

        private async Task CargarOngsAsync()
        {
            var ongs = await _http.GetFromJsonAsync<List<OngDto>>("/api/Ongs") ?? new();
            OngOptions = ongs.Select(o => new SelectListItem(o.NombreOng, o.IdOng.ToString())).ToList();
        }

        private async Task<List<InscripcionDto>> BuscarInscripcionesDeUsuario(int usuarioId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<InscripcionDto>>($"/api/Inscripciones/por-usuario/{usuarioId}") ?? new();
            }
            catch { return new(); }
        }

        // NUEVO: consulta al endpoint de estado (evita GetFromJsonAsync directo para no explotar con 404)
        private async Task<EstadoAsistenciaDto?> BuscarEstadoAsistencia(int idInscripcion)
        {
            try
            {
                var resp = await _http.GetAsync($"/api/Asistencias/estado/{idInscripcion}");
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<EstadoAsistenciaDto>();
            }
            catch { return null; }
        }

        // ---------- DTOs ----------
        public class UsuarioDto
        {
            public int IdUsuario { get; set; }
            public string Nombre { get; set; } = "";
            public string Apellido { get; set; } = "";
            public string CorreoUsuario { get; set; } = "";
        }

        public class OngDto
        {
            public int IdOng { get; set; }
            public string NombreOng { get; set; } = "";
        }

        public class InscripcionDto
        {
            public int IdInscripcion { get; set; }
            public int IdActividad { get; set; }
            public string NombreActividad { get; set; } = "";
            public DateTime FechaActividad { get; set; }
            public string? Lugar { get; set; }
            public string EstadoInscripcion { get; set; } = "";
            public bool YaTieneCertificado { get; set; } = false;
        }

        // Estado devuelto por /api/Asistencias/estado/{id}
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

        public class InscripcionVm
        {
            public int IdInscripcion { get; set; }
            public int IdActividad { get; set; }
            public string NombreActividad { get; set; } = "";
            public DateTime FechaActividad { get; set; }
            public string? Lugar { get; set; }
            public string EstadoInscripcion { get; set; } = "";

            // Progreso (desde /estado)
            public int DiasRequeridos { get; set; }
            public int DiasRegistrados { get; set; }
            public bool Completado { get; set; }

            // Elegibilidad
            public bool ElegibleCertificado { get; set; }
        }
    }
}
