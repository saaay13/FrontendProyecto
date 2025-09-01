using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Admin.Reconocimientos
{
    public class ReconocimientoInputC
    {
        public int IdCarnet { get; set; }

        [Required(ErrorMessage = "El voluntario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un voluntario válido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La ONG es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una ONG válida")]
        public int IdOng { get; set; }

        [Required(ErrorMessage = "La fecha de emisión es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaEmision { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; } = DateTime.Today.AddYears(1);

        [Required(ErrorMessage = "Los beneficios son obligatorios")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string Beneficios { get; set; } = string.Empty;

      
        public Guid CodigoVerificacion { get; set; } = Guid.NewGuid();

    
        public string EstadoInscripcion { get; set; } = "Activo";
    }
}
