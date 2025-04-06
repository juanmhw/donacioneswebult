namespace donacionesWeb.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string Email { get; set; } = null!;
        public string Contrasena { get; set; } = null!;
        public string TipoUsuario { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string? Telefono { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }
}
