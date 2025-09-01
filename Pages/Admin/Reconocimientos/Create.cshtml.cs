using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using FrontendProyecto.Pages.Admin.Reconocimientos;

public class AdminReconocimientosCreateModel : PageModel
{
    private readonly HttpClient _http;
    public AdminReconocimientosCreateModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

    [BindProperty] public ReconocimientoInput Input { get; set; } = new();

    public async Task OnGet()
    {
        await CargarCombos();
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
            IdUsuario = Input.IdUsuario,
            IdActividad = Input.IdActividad,
            FechaEmision = Input.FechaEmision
           
        };

        var resp = await _http.PostAsJsonAsync("api/Certificados", payload);
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

    // Dtos mínimos para combos
    public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    public record ActividadDto(int IdActividad, string NombreActividad, DateTime FechaActividad);
}
