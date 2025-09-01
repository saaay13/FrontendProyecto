using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Admin.Reconocimientos
{
    public class ReconocimientoInputC : IValidatableObject
    {
        public int IdCarnet { get; set; }

        [Required(ErrorMessage = "El voluntario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un voluntario válido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La ONG es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una ONG válida")]
        public int IdOng { get; set; }

        // Opcionales: el backend puede poner los defaults si van null
        [DataType(DataType.Date)]
        public DateTime? FechaEmision { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }

        [Required(ErrorMessage = "Los beneficios son obligatorios")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string Beneficios { get; set; } = string.Empty;

        // Lo genera el backend: si envías null, el backend asigna Guid.NewGuid()
        public Guid? CodigoVerificacion { get; set; }

        // Enum serializado como string hacia la API
        [Required]
        public string EstadoInscripcion { get; set; } = "Activo"; // "Activo" | "Suspendido" | "Vencido"

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Si el usuario decide fijar fechas, valida el rango
            if (FechaEmision.HasValue && FechaVencimiento.HasValue &&
                FechaVencimiento.Value.Date <= FechaEmision.Value.Date)
            {
                yield return new ValidationResult(
                    "La fecha de vencimiento debe ser mayor a la fecha de emisión.",
                    new[] { nameof(FechaVencimiento) });
            }

            var permitidos = new[] { "Activo", "Suspendido", "Vencido" };
            if (Array.IndexOf(permitidos, EstadoInscripcion) < 0)
            {
                yield return new ValidationResult(
                    "Estado inválido. Use: Activo, Suspendido o Vencido.",
                    new[] { nameof(EstadoInscripcion) });
            }
        }
    }
}
