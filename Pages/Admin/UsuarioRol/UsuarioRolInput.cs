using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    public class UsuarioRolInput
    {
        [Required(ErrorMessage = "Seleccione un usuario")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Seleccione un rol")]
        public int IdRol { get; set; }
    }
}
