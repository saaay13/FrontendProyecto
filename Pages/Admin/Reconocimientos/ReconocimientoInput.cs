using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Admin.Reconocimientos
{
    public class ReconocimientoInput
    {
        public int IdCertificado { get; set; }

        [Required(ErrorMessage = "El voluntario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un voluntario válido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La actividad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una actividad válida")]
        public int IdActividad { get; set; }

        [Required(ErrorMessage = "La fecha de emisión es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaEmision { get; set; } = DateTime.Today;

    
        public string? CodigoVerificacion { get; set; }
    }
}
