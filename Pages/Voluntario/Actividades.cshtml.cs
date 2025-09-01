using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace FrontendProyecto.Pages.Voluntario.Actividades
{
    [Authorize(Roles = "Voluntario")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        public IndexModel(IHttpClientFactory factory) => _http = factory.CreateClient("API"); // Program.cs: AddHttpClient("API", ...)

        public List<Item> Lista { get; set; } = new();

        public class Item
        {
            public int IdActividad { get; set; }
            public string? NombreProyecto { get; set; }   // viene dentro de Proyecto en el endpoint public (ver abajo)
            public string NombreActividad { get; set; } = "";
            public DateTime FechaActividad { get; set; }
            public TimeSpan HoraInicio { get; set; }
            public TimeSpan HoraFin { get; set; }
            public string Lugar { get; set; } = "";
            public int CupoMaximo { get; set; }
            public string EstadoActividad { get; set; } = ""; // gracias a JsonStringEnumConverter, llega como string
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Obtenemos las actividades públicas desde hoy en adelante
            var raw = await _http.GetFromJsonAsync<List<PublicActividadDto>>("api/Actividades/public");
            var data = raw ?? new();

            // Aplanamos para la tabla (NombreProyecto está dentro de Proyecto)
            Lista = data.Select(a => new Item
            {
                IdActividad = a.IdActividad,
                NombreActividad = a.NombreActividad ?? "",
                FechaActividad = a.FechaActividad,
                HoraInicio = a.HoraInicio,
                HoraFin = a.HoraFin,
                Lugar = a.Lugar ?? "",
                CupoMaximo = a.CupoMaximo,
                EstadoActividad = a.EstadoActividad ?? "",
                NombreProyecto = a.Proyecto?.NombreProyecto
            }).ToList();

            return Page();
        }

        // Handler para el botón Inscribir
        public async Task<IActionResult> OnPostInscribirAsync(int idActividad)
        {
            // POST /api/Actividades/{id}/inscribir
            // El backend obtiene IdUsuario del token (mejor), si no lo tienes aún, puedes enviar {} como body.
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/Actividades/{idActividad}/inscribir")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            var resp = await _http.SendAsync(request);
            if (resp.IsSuccessStatusCode)
            {
                TempData["ok"] = "Inscripción realizada.";
            }
            else
            {
                var msg = await resp.Content.ReadAsStringAsync();
                TempData["err"] = string.IsNullOrWhiteSpace(msg) ? "No se pudo inscribir." : msg;
            }

            return RedirectToPage();
        }

        // ====== DTO que refleja el shape del endpoint public ======
        public class PublicActividadDto
        {
            public int IdActividad { get; set; }
            public string? NombreActividad { get; set; }
            public DateTime FechaActividad { get; set; }
            public TimeSpan HoraInicio { get; set; }
            public TimeSpan HoraFin { get; set; }
            public string? Lugar { get; set; }
            public int CupoMaximo { get; set; }
            public string? EstadoActividad { get; set; }
            public PublicProyectoDto? Proyecto { get; set; }

            public class PublicProyectoDto
            {
                public int IdProyecto { get; set; }
                public string? NombreProyecto { get; set; }
            }
        }
    }
}
