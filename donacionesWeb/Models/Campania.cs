using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace donacionesWeb.Models
{
    public class Campania
    {
        public int CampaniaId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [JsonPropertyName("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateTime? FechaFin { get; set; }

        [Required(ErrorMessage = "La meta de recaudación es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La meta debe ser mayor a 0")]
        [JsonPropertyName("metaRecaudacion")]
        public decimal MetaRecaudacion { get; set; }

        [JsonPropertyName("montoRecaudado")]
        public decimal? MontoRecaudado { get; set; }

        [Required(ErrorMessage = "El ID del creador es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "ID de creador inválido")]
        [JsonPropertyName("usuarioIdcreador")]
        public int UsuarioIdcreador { get; set; }

        [JsonPropertyName("activa")]
        public bool? Activa { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime? FechaCreacion { get; set; }
    }
}
