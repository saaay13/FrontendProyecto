using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Ongs
{
    public class OngInput
    {
        [Required, StringLength(150)]
        public string NombreOng { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        [Phone, StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress, StringLength(150)]
        public string? Correo { get; set; }
    }
}
