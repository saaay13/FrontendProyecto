using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using BackendProyecto.Models;
namespace FrontendProyecto.Pages.User
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        public List<Usuarios> usuarios { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Usuarios");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    usuarios = JsonSerializer.Deserialize<List<Usuarios>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Usuarios>();

                    return Page();
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                // Otros códigos de estado
                return StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible.");
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al cargar los clientes.");
                return Page();
            }
        }
    }
}
