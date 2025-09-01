using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using FrontendProyecto.Pages.Admin.Reconocimientos;

public class AdminReconocimientosEditModel : PageModel
{
    private readonly HttpClient _http;
    public AdminReconocimientosEditModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

    [BindProperty] public ReconocimientoInput Input { get; set; } = new();

    public async Task<IActionResult> OnGet(int id)
    {
        var c = await _http.GetFromJsonAsync<CertificadoDto>($"api/Certificados/{id}");
        if (c is null) return NotFound();

        Input = new ReconocimientoInput
        {
            IdCertificado = c.IdCertificado,
            IdUsuario = c.IdUsuario,
            IdActividad = c.IdActividad,
            FechaEmision = c.FechaEmision,
            CodigoVerificacion = c.CodigoVerificacion
        };

        await CargarCombos();
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            await CargarCombos();
            return Page();
        }

       
        var payload = new
        {
            IdCertificado = Input.IdCertificado,
            IdUsuario = Input.IdUsuario,
            IdActividad = Input.IdActividad,
            FechaEmision = Input.FechaEmision
            
        };

        var resp = await _http.PutAsJsonAsync($"api/Certificados/{Input.IdCertificado}", payload);
        if (resp.IsSuccessStatusCode) return RedirectToPage("./Index");

        ModelState.AddModelError(string.Empty, await resp.Content.ReadAsStringAsync());
        await CargarCombos();
        return Page();
    }

    private async Task CargarCombos()
    {
        var usuarios = await _http.GetFromJsonAsync<List<UsuarioDto>>("api/Usuarios") ?? new();
        var actividades = await _http.GetFromJsonAsync<List<ActividadDto>>("api/Actividades") ?? new();

        ViewData["UsuarioOptions"] = usuarios
            .Select(u => new SelectListItem($"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})", u.IdUsuario.ToString()))
            .ToList();

        ViewData["ActividadOptions"] = actividades
            .Select(a => new SelectListItem($"{a.NombreActividad} ({a.FechaActividad:yyyy-MM-dd})", a.IdActividad.ToString()))
            .ToList();
    }

    // DTOs 
    public class CertificadoDto
    {
        public int IdCertificado { get; set; }
        public int IdUsuario { get; set; }
        public int IdActividad { get; set; }
        public DateTime FechaEmision { get; set; }
        public string CodigoVerificacion { get; set; } = "";
    }

    public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    public record ActividadDto(int IdActividad, string NombreActividad, DateTime FechaActividad);
}
