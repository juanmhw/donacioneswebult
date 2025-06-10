namespace donacionesWeb.Models
{
    public class RespuestaMensaje
    {
        public int RespuestaId { get; set; }
        public int MensajeId { get; set; }
        public int UsuarioId { get; set; }
        public string Contenido { get; set; } = null!;
        public DateTime? FechaRespuesta { get; set; }

        public string? NombreUsuario { get; set; }
    }
}
