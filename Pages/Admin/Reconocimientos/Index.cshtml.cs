using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

public class AdminReconocimientosIndexModel : PageModel
{
    private readonly HttpClient _http;
    public AdminReconocimientosIndexModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

   
    public List<CertificadoVm> Certificados { get; set; } = new();
    public List<CarnetVm> Carnets { get; set; } = new();

    public async Task OnGet()
    {
        await CargarCertificados();
        await CargarCarnets();
    }

    public async Task<IActionResult> OnPostAnularCert(int id)
    {
        var resp = await _http.DeleteAsync($"api/Certificados/{id}");
        if (!resp.IsSuccessStatusCode)
            ModelState.AddModelError(string.Empty, $"No se pudo anular: {await resp.Content.ReadAsStringAsync()}");

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAnularCarnet(int id)
    {
        var resp = await _http.DeleteAsync($"api/Carnets/{id}");
        if (!resp.IsSuccessStatusCode)
            ModelState.AddModelError(string.Empty, $"No se pudo eliminar: {await resp.Content.ReadAsStringAsync()}");

        return RedirectToPage();
    }

    private async Task CargarCertificados()
    {
        
        var data = await _http.GetFromJsonAsync<List<CertificadoDto>>("api/Certificados") ?? new();
        Certificados = data.Select(c => new CertificadoVm
        {
            IdCertificado = c.IdCertificado,
            CodigoVerificacion = c.CodigoVerificacion,
            FechaEmision = c.FechaEmision,
            UsuarioNombre = c.Usuario is null ? "-" : $"{c.Usuario.Nombre} {c.Usuario.Apellido}",
            ProyectoNombre = c.Actividad?.Proyecto?.NombreProyecto ?? "-",
            ActividadNombre = c.Actividad?.NombreActividad ?? "-",
            OngNombre = c.Actividad?.Proyecto?.Ong?.NombreOng ?? "-"
        }).ToList();
    }

    private async Task CargarCarnets()
    {
    
        var data = await _http.GetFromJsonAsync<List<CarnetDto>>("api/Carnets") ?? new();
        Carnets = data.Select(k => new CarnetVm
        {
            IdCarnet = k.IdCarnet,
            CodigoVerificacion = k.CodigoVerificacion.ToString(),
            FechaEmision = k.FechaEmision,
            FechaVencimiento = k.FechaVencimiento,
            EstadoInscripcion = k.EstadoInscripcion.ToString(),
            UsuarioNombre = k.Usuario is null ? "-" : $"{k.Usuario.Nombre} {k.Usuario.Apellido}",
            OngNombre = k.Ong?.NombreOng ?? "-"
        }).ToList();
    }

    // ====== DTOs 
    public class CertificadoDto
    {
        public int IdCertificado { get; set; }
        public int IdUsuario { get; set; }
        public int IdActividad { get; set; }
        public DateTime FechaEmision { get; set; }
        public string CodigoVerificacion { get; set; } = "";

        public UsuarioDto? Usuario { get; set; }
        public ActividadDto? Actividad { get; set; }
    }

    public class CarnetDto
    {
        public int IdCarnet { get; set; }
        public int IdUsuario { get; set; }
        public int IdOng { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Beneficios { get; set; } = "";
        public Guid CodigoVerificacion { get; set; }
        public string EstadoInscripcion { get; set; } = ""; // si vuelve como string; si es número, usa int/enum

        public UsuarioDto? Usuario { get; set; }
        public OngDto? Ong { get; set; }
    }

    public class UsuarioDto { public string Nombre { get; set; } = ""; public string Apellido { get; set; } = ""; }
    public class OngDto { public string NombreOng { get; set; } = ""; }
    public class ProyectoDto { public string? NombreProyecto { get; set; }
        public OngDto? Ong { get; set; } }
    public class ActividadDto { public string? NombreActividad { get; set; } 
        public ProyectoDto? Proyecto { get; set; } }

    // ====== ViewModels para la tabla ======
    public class CertificadoVm
    {
        public int IdCertificado { get; set; }
        public string CodigoVerificacion { get; set; } = "";
        public DateTime FechaEmision { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string ProyectoNombre { get; set; } = "";
        public string ActividadNombre { get; set; } = "";
        public string OngNombre { get; set; } = "";
    }

    public class CarnetVm
    {
        public int IdCarnet { get; set; }
        public string CodigoVerificacion { get; set; } = "";
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string EstadoInscripcion { get; set; } = "";
        public string UsuarioNombre { get; set; } = "";
        public string OngNombre { get; set; } = "";
    }
}
