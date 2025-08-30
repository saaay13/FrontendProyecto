using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    public class UsuarioRolInput
    {
        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdRol { get; set; }
    }
}
