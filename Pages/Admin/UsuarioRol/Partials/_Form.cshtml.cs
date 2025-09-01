using System.Collections.Generic;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    public class UsuarioRolFormModel
    {
        public UsuarioRolInput Input { get; set; } = new();
        public List<UsuarioItem> Usuarios { get; set; } = new();
        public List<RolItem> Roles { get; set; } = new();

        public bool ReadOnlyUsuario { get; set; } = false;
    }

    public record UsuarioItem(int IdUsuario, string Nombre, string CorreoUsuario);
    public record RolItem(int IdRol, string NombreRol);
}
