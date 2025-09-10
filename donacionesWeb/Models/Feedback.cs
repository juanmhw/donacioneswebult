using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace donacionesWeb.Models
{
    public class Feedback
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; } = 4; // Valor predeterminado según la API

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
        [JsonPropertyName("texto")]
        public string Texto { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        [JsonPropertyName("calificacion")]
        public int Calificacion { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedad adicional para la vista pero no para la API
        [Required(ErrorMessage = "El asunto es obligatorio")]
        [StringLength(200, ErrorMessage = "El asunto no puede exceder 200 caracteres")]
        [JsonIgnore]
        public string Asunto { get; set; } = string.Empty;
    }
}