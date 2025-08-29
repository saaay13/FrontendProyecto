using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendProyecto.Pages.Coordinador
{

    [Authorize(Roles = "Coordinador")]
    public class PanelModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public PanelModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public int MisProyectos { get; set; }
        public int ActividadesProximas { get; set; }

        public List<ActividadDto> TopActividades { get; set; } = new();

        public async Task OnGet()
        {
            var client = _httpClientFactory.CreateClient("API");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var proyectos = await client.GetFromJsonAsync<List<ProyectoDto>>("/api/Proyectos") ?? new();
            var mios = proyectos.Where(p => p.IdResponsable.ToString() == userId).ToList();
            MisProyectos = mios.Count;

            var actividades = await client.GetFromJsonAsync<List<ActividadDto>>("/api/Actividades") ?? new();
            var actsMias = actividades.Where(a => mios.Any(p => p.IdProyecto == a.IdProyecto)).ToList();

            ActividadesProximas = actsMias.Count(a => a.FechaActividad >= DateTime.Today);
            TopActividades = actsMias.Where(a => a.FechaActividad >= DateTime.Today)
                                     .OrderBy(a => a.FechaActividad).Take(5).ToList();
        }

        public record ProyectoDto(int IdProyecto, int IdOng, string NombreProyecto, int IdResponsable);
        public record ActividadDto(int IdActividad, int IdProyecto, string NombreActividad, DateTime FechaActividad, string Lugar, string EstadoActividad);
    }

}
