using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace donacionesWeb.Models.ViewModels
{
    public class CampaniaViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "La meta de recaudación es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La meta debe ser mayor a 0")]
        public decimal MetaRecaudacion { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}
