namespace donacionesWeb.Models
{
    public class Mensaje
    {
        public int MensajeId { get; set; }
        public int UsuarioOrigen { get; set; }
        public int? UsuarioDestino { get; set; }
        public string Asunto { get; set; } = null!;
        public string Contenido { get; set; } = null!;
        public DateTime? FechaEnvio { get; set; }
        public bool? Leido { get; set; }
        public bool? Respondido { get; set; }
    }
}
