using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using FrontendProyecto.Pages.Admin.Reconocimientos;

public class AdminReconocimientosEditCModel : PageModel
{
    private readonly HttpClient _http;
    public AdminReconocimientosEditCModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

    [BindProperty] public ReconocimientoInputC Input { get; set; } = new();

    public async Task<IActionResult> OnGet(int id)
    {
      
        var data = await _http.GetFromJsonAsync<List<CarnetDto>>("api/Carnets") ?? new();
        var k = data.FirstOrDefault(x => x.IdCarnet == id);
        if (k is null) return NotFound();

        Input = new ReconocimientoInputC
        {
            IdCarnet = k.IdCarnet,
            IdUsuario = k.IdUsuario,
            IdOng = k.IdOng,
            FechaEmision = k.FechaEmision,
            FechaVencimiento = k.FechaVencimiento,
            Beneficios = k.Beneficios,
            CodigoVerificacion = k.CodigoVerificacion
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
            IdCarnet = Input.IdCarnet,
            IdUsuario = Input.IdUsuario,
            IdOng = Input.IdOng,
            FechaEmision = Input.FechaEmision,
            FechaVencimiento = Input.FechaVencimiento,
            Beneficios = Input.Beneficios,
            CodigoVerificacion = Input.CodigoVerificacion
           
        };

        var resp = await _http.PutAsJsonAsync($"api/Carnets/{Input.IdCarnet}", payload);
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

    // DTOs 
    public class CarnetDto
    {
        public int IdCarnet { get; set; }
        public int IdUsuario { get; set; }
        public int IdOng { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Beneficios { get; set; } = "";
        public Guid CodigoVerificacion { get; set; }
        public string EstadoInscripcion { get; set; } = "";

        public UsuarioDto? Usuario { get; set; }
        public OngDto? Ong { get; set; }
    }

    public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    public record OngDto(int IdOng, string NombreOng);
}
