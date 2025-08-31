using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

public class AdminConfiguracionIndexModel : PageModel
{
    private readonly HttpClient _http;
    public AdminConfiguracionIndexModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

    public List<RolDto> Roles { get; set; } = new();

    public async Task OnGet()
    {
        // Tu endpoint actual: GET api/Rol
        Roles = await _http.GetFromJsonAsync<List<RolDto>>("api/Rol") ?? new();
    }

    public record RolDto(int IdRol, string NombreRol);
}
