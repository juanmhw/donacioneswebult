using System;
using System.ComponentModel.DataAnnotations;

namespace donacionesWeb.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Contrasena { get; set; } = null!;

        [Required]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Apellido { get; set; } = null!;

        public string? Telefono { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public string? ImagenUrl { get; set; } 
    }
}