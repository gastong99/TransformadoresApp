using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransformadoresApp.Models
{
    public class Material
    {
        [Key]
        public int MaterialId { get; set; }

        [Required(ErrorMessage = "El código del material es obligatorio.")]
        [StringLength(50, ErrorMessage = "El código no puede tener más de 50 caracteres.")]
        [Display(Name = "Código del material")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del material es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
        [Display(Name = "Nombre del material")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una unidad de medida.")]
        [Display(Name = "Unidad de medida")]
        public int UnitOfMeasureId { get; set; }

        [ForeignKey(nameof(UnitOfMeasureId))]
        public UnitOfMeasure? UnitOfMeasure { get; set; }
    }
}


