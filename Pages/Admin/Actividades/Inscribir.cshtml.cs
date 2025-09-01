using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class InscribirModel : PageModel
    {
        private readonly HttpClient _http;
        public InscribirModel(IHttpClientFactory httpFactory) => _http = httpFactory.CreateClient("API");

        [BindProperty(SupportsGet = true)]
        public int IdActividad { get; set; }

        public string NombreActividad { get; set; } = "";
        public List<SelectListItem> UsuariosOptions { get; set; } = new();
        public List<InscripcionItem> Inscripciones { get; set; } = new();

        [BindProperty]
        public int IdUsuarioInscribir { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdActividad <= 0)
            {
                TempData["err"] = "Id de actividad inválido.";
                return RedirectToPage("/Admin/Actividades/Index");
            }

         
            var respAct = await _http.GetAsync($"/api/Actividades/{IdActividad}");
            if (respAct.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["err"] = "Actividad no encontrada.";
                return RedirectToPage("/Admin/Actividades/Index");
            }
            respAct.EnsureSuccessStatusCode();
            var act = await respAct.Content.ReadFromJsonAsync<ActividadDto>();
            if (act is null)
            {
                TempData["err"] = "No se pudo cargar la actividad.";
                return RedirectToPage("/Admin/Actividades/Index");
            }
            NombreActividad = act.NombreActividad;

         
            var usuarios = await _http.GetFromJsonAsync<List<UsuarioDto>>("/api/Usuarios") ?? new();
            UsuariosOptions = usuarios.Select(u =>
                new SelectListItem($"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})", u.IdUsuario.ToString())).ToList();

            var respInsc = await _http.GetAsync($"/api/Inscripciones/por-actividad/{IdActividad}");
            if (respInsc.IsSuccessStatusCode)
            {
                Inscripciones = await respInsc.Content.ReadFromJsonAsync<List<InscripcionItem>>() ?? new();
            }
            else
            {
                TempData["err"] = $"No se pudieron cargar las inscripciones (HTTP {(int)respInsc.StatusCode}).";
                Inscripciones = new();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAgregarAsync()
        {
            var body = new { IdUsuario = IdUsuarioInscribir, IdActividad, EstadoInscripcion = "Confirmada" };
            var resp = await _http.PostAsJsonAsync("/api/Inscripciones", body);

            TempData[resp.IsSuccessStatusCode ? "ok" : "err"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage(new { IdActividad });
        }

        public async Task<IActionResult> OnPostCambiarEstadoAsync(int idInscripcion, string nuevoEstado)
        {
            var body = new { IdInscripcion = idInscripcion, EstadoInscripcion = nuevoEstado };
            var resp = await _http.PutAsJsonAsync($"/api/Inscripciones/{idInscripcion}", body);

            TempData[resp.IsSuccessStatusCode ? "ok" : "err"] = await resp.Content.ReadAsStringAsync();
            return RedirectToPage(new { IdActividad });
        }

        // DTOs
        public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
        public record ActividadDto(int IdActividad, string NombreActividad, DateTime FechaActividad, string? Lugar);
        public class InscripcionItem
        {
            public int IdInscripcion { get; set; }
            public int IdUsuario { get; set; }
            public string NombreUsuario { get; set; } = "";
            public string EstadoInscripcion { get; set; } = "";
            public DateTime FechaInscripcion { get; set; }
        }
    }
}
