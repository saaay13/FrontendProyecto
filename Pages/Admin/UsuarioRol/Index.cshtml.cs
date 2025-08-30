using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Usuarios
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;

        public IndexModel(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("API");
        }

        public List<UsuarioRolDto> Lista { get; set; } = new();

        public async Task OnGetAsync()
        {
            Lista = await _http.GetFromJsonAsync<List<UsuarioRolDto>>("/api/UsuarioRol")
                    ?? new List<UsuarioRolDto>();
        }

        public record UsuarioRolDto(
             int IdUsuario,
             int IdRol,
             UsuarioDto? Usuario,
             RolDto? Rol,
             DateTime FechaAsignacion
         );

        public record UsuarioDto(
            int IdUsuario,
            string Nombre,
            string Apellido,
            string CorreoUsuario,
            DateTime FechaRegistro
        );

        public record RolDto(
            int IdRol,
            string NombreRol
        );


    }
}
