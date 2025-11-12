using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(50, ErrorMessage = "El código no puede tener más de 50 caracteres.")]
        [Display(Name = "Código del transformador")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
        [Display(Name = "Nombre del transformador")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "La potencia es obligatoria.")]
        [Display(Name = "Potencia (kVA)")]
        [Range(1, 5000, ErrorMessage = "La potencia debe estar entre 1 y 5000 kVA.")]
        public int? PotenciaKVA { get; set; }

        [Required(ErrorMessage = "Las pérdidas en el hierro son obligatorias.")]
        [Display(Name = "Pérdidas en el hierro (Po) [W]")]
        [Range(0, 10000, ErrorMessage = "Las pérdidas deben estar entre 0 y 10000 W.")]
        public int? PerdidasPo { get; set; }

        [Required(ErrorMessage = "Las pérdidas en el cobre son obligatorias.")]
        [Display(Name = "Pérdidas en el cobre (Pcc) [W]")]
        [Range(0, 10000, ErrorMessage = "Las pérdidas deben estar entre 0 y 10000 W.")]
        public int? PerdidasPcc { get; set; }

        [Required(ErrorMessage = "La tensión de cortocircuito es obligatoria.")]
        [Display(Name = "Tensión de cortocircuito (%)")]
        [Range(0, 100, ErrorMessage = "El valor debe estar entre 0 y 100%.")]
        public double? Ucc { get; set; }

        [Required(ErrorMessage = "El largo es obligatorio.")]
        [Display(Name = "Largo (mm)")]
        [Range(0, 10000, ErrorMessage = "El largo debe estar entre 0 y 10000 mm.")]
        public int? Largo { get; set; }

        [Required(ErrorMessage = "El ancho es obligatorio.")]
        [Display(Name = "Ancho (mm)")]
        [Range(0, 10000, ErrorMessage = "El ancho debe estar entre 0 y 10000 mm.")]
        public int? Ancho { get; set; }

        [Required(ErrorMessage = "El alto es obligatorio.")]
        [Display(Name = "Alto (mm)")]
        [Range(0, 10000, ErrorMessage = "El alto debe estar entre 0 y 10000 mm.")]
        public int? Alto { get; set; }

        [Required(ErrorMessage = "El diámetro es obligatorio.")]
        [Display(Name = "Diámetro (mm)")]
        [Range(0, 10000, ErrorMessage = "El diámetro debe estar entre 0 y 10000 mm.")]
        public int? Diametro { get; set; }

        [Required(ErrorMessage = "El peso es obligatorio.")]
        [Display(Name = "Peso (Kg)")]
        [Range(0, 10000, ErrorMessage = "El peso debe estar entre 0 y 10000 kg.")]
        public int? Peso { get; set; }


        public ICollection<BomItem>? BomItems { get; set; }
    }
}


