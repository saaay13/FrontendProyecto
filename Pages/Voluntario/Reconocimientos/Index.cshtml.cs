using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Voluntario.Reconocimientos
{
    [Authorize(Roles = "Voluntario,Administrador")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        public IndexModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        public List<CertVm> Certificados { get; set; } = new();
        public List<CarnetVm> Carnets { get; set; } = new();

        public async Task OnGet()
        {
            Carnets = await _http.GetFromJsonAsync<List<CarnetVm>>("/api/Carnets/mios") ?? new();
            Certificados = await _http.GetFromJsonAsync<List<CertVm>>("/api/Certificados/mios") ?? new();
        }

        public class CarnetVm
        {
            public int IdCarnet { get; set; }
            public string OngNombre { get; set; } = "";
            public DateTime FechaEmision { get; set; }
            public DateTime? FechaVencimiento { get; set; }
            public bool? Descargado { get; set; } 
        }

        public class CertVm
        {
            public int IdCertificado { get; set; }
            public string OngNombre { get; set; } = "";
            public string ProyectoNombre { get; set; } = "";
            public string ActividadNombre { get; set; } = "";
            public DateTime FechaEmision { get; set; }
            public bool? Descargado { get; set; }
        }
    }
}
