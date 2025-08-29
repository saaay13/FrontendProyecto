using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;

namespace FrontendProyecto.Pages.Voluntario
{
    [Authorize(Roles = "Voluntario")]
    public class PanelModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PanelModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ActividadDto> ProximasActividades { get; private set; } = new();
        public List<CertificadoDto> MisCertificados { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("API");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Cargar actividades
            try
            {
                var acts = await client.GetFromJsonAsync<List<ActividadDto>>("/api/Actividades") ?? new();
                ProximasActividades = acts
                    .Where(a => a.FechaActividad.Date >= DateTime.Today)
                    .OrderBy(a => a.FechaActividad)
                    .Take(5)
                    .ToList();
            }
            catch { /* opcional: log */ }

            // Cargar certificados del usuario
            try
            {
                var certs = await client.GetFromJsonAsync<List<CertificadoDto>>("/api/Certificados") ?? new();
                MisCertificados = certs
                    .Where(c => c.IdUsuario.ToString() == userId)
                    .OrderByDescending(c => c.FechaEmision)
                    .Take(5)
                    .ToList();
            }
            catch { /* opcional: log */ }
        }

        public record ActividadDto(
            int IdActividad,
            int IdProyecto,
            string NombreActividad,
            DateTime FechaActividad,
            string Lugar,
            string EstadoActividad
        );

        public record CertificadoDto(
            int IdCertificado,
            int IdUsuario,
            int IdActividad,
            DateTime FechaEmision,
            string CodigoVerificacion
        );
    }
}
