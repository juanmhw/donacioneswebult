namespace donacionesWeb.Models
{
    public class UsuarioRol
    {
        public int UsuarioRolId { get; set; }
        public int UsuarioId { get; set; }
        public int RolId { get; set; }
        public DateTime FechaAsignacion { get; set; }

      
    }
}
