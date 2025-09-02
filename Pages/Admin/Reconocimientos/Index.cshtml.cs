using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;

[Authorize(Roles = "Administrador,Coordinador")]
public class AdminReconocimientosIndexModel : PageModel
{
    private readonly HttpClient _http;
    public AdminReconocimientosIndexModel(IHttpClientFactory factory)
        => _http = factory.CreateClient("API");

    public List<CertificadoVm> Certificados { get; set; } = new();
    public List<CarnetVm> Carnets { get; set; } = new();

    private Dictionary<int, (string Nombre, string Apellido)> _usuarios = new();
    private Dictionary<int, string> _ongs = new();
    private Dictionary<int, (string NombreActividad, int? IdProyecto)> _actividades = new();
    private Dictionary<int, (string NombreProyecto, int? IdOng)> _proyectos = new();

    public async Task OnGetAsync()
    {
        await CargarCatalogosFallback();
        await CargarCertificados();
        await CargarCarnets();
    }

    public async Task<IActionResult> OnPostAnularCert(int id)
    {
        var resp = await _http.DeleteAsync($"/api/Certificados/{id}");
        if (!resp.IsSuccessStatusCode)
            ModelState.AddModelError(string.Empty, $"No se pudo anular: {await resp.Content.ReadAsStringAsync()}");

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAnularCarnet(int id)
    {
        var resp = await _http.DeleteAsync($"/api/Carnets/{id}");
        if (!resp.IsSuccessStatusCode)
            ModelState.AddModelError(string.Empty, $"No se pudo eliminar: {await resp.Content.ReadAsStringAsync()}");

        return RedirectToPage();
    }

    private async Task CargarCertificados()
    {
        var data = await _http.GetFromJsonAsync<List<CertificadoDto>>("/api/Certificados") ?? new();

        Certificados = data.Select(c =>
        {
            var usuarioNombre = c.Usuario is null
                ? ResolverUsuario(c.IdUsuario)
                : $"{c.Usuario.Nombre} {c.Usuario.Apellido}";

            string actividadNombre = c.Actividad?.NombreActividad ?? ResolverActividad(c.IdActividad).NombreActividad;
            string proyectoNombre = c.Actividad?.Proyecto?.NombreProyecto
                ?? ResolverProyecto(ResolverActividad(c.IdActividad).IdProyecto).NombreProyecto;
            string ongNombre = c.Actividad?.Proyecto?.Ong?.NombreOng
                ?? ResolverOng(ResolverProyecto(ResolverActividad(c.IdActividad).IdProyecto).IdOng);

            return new CertificadoVm
            {
                IdCertificado = c.IdCertificado,
                CodigoVerificacion = c.CodigoVerificacion,
                FechaEmision = c.FechaEmision,
                UsuarioNombre = usuarioNombre,
                ProyectoNombre = proyectoNombre,
                ActividadNombre = actividadNombre,
                OngNombre = ongNombre
            };
        }).ToList();
    }

    private async Task CargarCarnets()
    {
        var data = await _http.GetFromJsonAsync<List<CarnetDto>>("/api/Carnets") ?? new();

        Carnets = data.Select(k =>
        {
            var usuarioNombre = k.Usuario is null
                ? ResolverUsuario(k.IdUsuario)
                : $"{k.Usuario.Nombre} {k.Usuario.Apellido}";

            var ongNombre = k.Ong?.NombreOng ?? ResolverOng(k.IdOng);

            return new CarnetVm
            {
                IdCarnet = k.IdCarnet,
                CodigoVerificacion = k.CodigoVerificacion.ToString(),
                FechaEmision = k.FechaEmision,
                FechaVencimiento = k.FechaVencimiento,
                Activo = k.Activo,
                UsuarioNombre = usuarioNombre,
                OngNombre = ongNombre
            };
        }).ToList();
    }

    private async Task CargarCatalogosFallback()
    {
        // Usuarios
        var usuarios = await _http.GetFromJsonAsync<List<UsuarioMin>>("/api/Usuarios") ?? new();
        _usuarios = usuarios.ToDictionary(
            u => u.IdUsuario, u => (u.Nombre, u.Apellido));

        var ongs = await _http.GetFromJsonAsync<List<OngMin>>("/api/Ongs") ?? new();
        _ongs = ongs.ToDictionary(o => o.IdOng, o => o.NombreOng);

        var acts = await _http.GetFromJsonAsync<List<ActividadMin>>("/api/Actividades") ?? new();
        _actividades = acts.ToDictionary(a => a.IdActividad, a => (a.NombreActividad ?? "-", (int?)a.IdProyecto));

        var proys = await _http.GetFromJsonAsync<List<ProyectoMin>>("/api/Proyectos") ?? new();
        _proyectos = proys.ToDictionary(p => p.IdProyecto, p => (p.NombreProyecto ?? "-", (int?)p.IdOng));
    }

    private (string NombreActividad, int? IdProyecto) ResolverActividad(int id)
        => _actividades.TryGetValue(id, out var v) ? v : ("-", null);

    private (string NombreProyecto, int? IdOng) ResolverProyecto(int? id)
        => (id is not null && _proyectos.TryGetValue(id.Value, out var v)) ? v : ("-", null);

    private string ResolverOng(int? id)
        => (id is not null && _ongs.TryGetValue(id.Value, out var nombre)) ? nombre : "-";

    private string ResolverUsuario(int idUsuario)
        => _usuarios.TryGetValue(idUsuario, out var u) ? $"{u.Nombre} {u.Apellido}" : "-";

    // ====== DTOs esperados por los GET ======
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
        public DateTime? FechaVencimiento { get; set; }  
        public Guid CodigoVerificacion { get; set; }
        public bool Activo { get; set; }             
        public UsuarioDto? Usuario { get; set; }
        public OngDto? Ong { get; set; }
    }

    public class UsuarioDto { public string Nombre { get; set; } = ""; public string Apellido { get; set; } = ""; }
    public class OngDto { public string NombreOng { get; set; } = ""; }
    public class ProyectoDto
    {
        public int IdProyecto { get; set; }
        public string? NombreProyecto { get; set; }
        public OngDto? Ong { get; set; }
    }
    public class ActividadDto
    {
        public int IdActividad { get; set; }
        public string? NombreActividad { get; set; }
        public ProyectoDto? Proyecto { get; set; }
    }

    // Catálogos mínimos
    public class UsuarioMin { public int IdUsuario { get; set; } public string Nombre { get; set; } = ""; public string Apellido { get; set; } = ""; }
    public class OngMin { public int IdOng { get; set; } public string NombreOng { get; set; } = ""; }
    public class ActividadMin { public int IdActividad { get; set; } public int IdProyecto { get; set; } public string? NombreActividad { get; set; } }
    public class ProyectoMin { public int IdProyecto { get; set; } public int IdOng { get; set; } public string? NombreProyecto { get; set; } }

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
        public DateTime? FechaVencimiento { get; set; }
        public bool Activo { get; set; }      // <- reemplaza EstadoInscripcion string
        public string UsuarioNombre { get; set; } = "";
        public string OngNombre { get; set; } = "";
    }
}
