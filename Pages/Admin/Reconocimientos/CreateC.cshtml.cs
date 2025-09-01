using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using FrontendProyecto.Pages.Admin.Reconocimientos;

public class AdminReconocimientosCreateCModel : PageModel
{
    private readonly HttpClient _http;
    public AdminReconocimientosCreateCModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

    [BindProperty] public ReconocimientoInputC Input { get; set; } = new();

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
            IdOng = Input.IdOng,
            FechaEmision = Input.FechaEmision,
            FechaVencimiento = Input.FechaVencimiento,
            Beneficios = Input.Beneficios,
            CodigoVerificacion = Input.CodigoVerificacion,
        };

        var resp = await _http.PostAsJsonAsync("api/Carnets", payload);
        if (resp.IsSuccessStatusCode) return RedirectToPage("./Index");

        ModelState.AddModelError(string.Empty, await resp.Content.ReadAsStringAsync());
        await CargarCombos();
        return Page();
    }

    private async Task CargarCombos()
    {
        var usuarios = await _http.GetFromJsonAsync<List<UsuarioDto>>("api/Usuarios") ?? new();
        var ongs = await _http.GetFromJsonAsync<List<OngDto>>("api/Ongs") ?? new();

        ViewData["UsuarioOptions"] = usuarios
            .Select(u => new SelectListItem($"{u.Nombre} {u.Apellido} ({u.CorreoUsuario})", u.IdUsuario.ToString()))
            .ToList();

        ViewData["OngOptions"] = ongs
            .Select(o => new SelectListItem(o.NombreOng, o.IdOng.ToString()))
            .ToList();
    }

    public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    public record OngDto(int IdOng, string NombreOng);
}
