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
        private const int UMBRAL_ASISTENCIA = 70;

        public EmitirModel(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("API");
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

            // 2) Calcular asistencia por inscripción
            foreach (var i in inscripciones)
            {
                var asistencias = await BuscarAsistenciasPorInscripcion(i.IdInscripcion);

                var total = asistencias.Count;
                var presentes = asistencias.Count(a => a.Asistio);
                var porcentaje = total == 0 ? 0 : (int)Math.Round(presentes * 100.0 / total);

                var elegibleCert = i.EstadoInscripcion == "Confirmada"
                                   && i.FechaActividad.Date <= DateTime.UtcNow.Date
                                   && porcentaje >= UMBRAL_ASISTENCIA
                                   && !i.YaTieneCertificado;

                InscripcionesVM.Add(new InscripcionVm
                {
                    IdInscripcion = i.IdInscripcion,
                    IdActividad = i.IdActividad,
                    NombreActividad = i.NombreActividad,
                    FechaActividad = i.FechaActividad,
                    Lugar = i.Lugar,
                    EstadoInscripcion = i.EstadoInscripcion,
                    TotalAsistencias = total,
                    Presentes = presentes,
                    Porcentaje = porcentaje,
                    ElegibleCertificado = elegibleCert
                });
            }

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

        private async Task<List<AsistenciaDto>> BuscarAsistenciasPorInscripcion(int idInscripcion)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<AsistenciaDto>>($"/api/Asistencias/por-inscripcion/{idInscripcion}") ?? new();
            }
            catch { return new(); }
        }

        // ---------- DTOs ----------
        public class UsuarioDto { public int IdUsuario { get; set; } public string Nombre { get; set; } = ""; public string Apellido { get; set; } = ""; public string CorreoUsuario { get; set; } = ""; }
        public class OngDto { public int IdOng { get; set; } public string NombreOng { get; set; } = ""; }
        public class InscripcionDto { public int IdInscripcion { get; set; } public int IdActividad { get; set; } public string NombreActividad { get; set; } = ""; public DateTime FechaActividad { get; set; } public string? Lugar { get; set; } public string EstadoInscripcion { get; set; } = ""; public bool YaTieneCertificado { get; set; } = false; }
        public class AsistenciaDto { public int IdAsistencia { get; set; } public bool Asistio { get; set; } public string? Observacion { get; set; } public DateTime HoraResgistro { get; set; } }
        public class InscripcionVm { public int IdInscripcion { get; set; } public int IdActividad { get; set; } public string NombreActividad { get; set; } = ""; public DateTime FechaActividad { get; set; } public string? Lugar { get; set; } public string EstadoInscripcion { get; set; } = ""; public int TotalAsistencias { get; set; } public int Presentes { get; set; } public int Porcentaje { get; set; } public bool ElegibleCertificado { get; set; } }
    }
}
