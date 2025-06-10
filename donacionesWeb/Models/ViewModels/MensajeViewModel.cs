namespace donacionesWeb.Models.ViewModels
{
    public class MensajeViewModel
    {
        public Mensaje Mensaje { get; set; } = new Mensaje();
        public List<RespuestaMensaje> Respuestas { get; set; } = new List<RespuestaMensaje>();
    }

    public class BandejaEntradaViewModel
    {
        public List<Mensaje> MensajesRecibidos { get; set; } = new List<Mensaje>();
        public List<Mensaje> MensajesEnviados { get; set; } = new List<Mensaje>();
    }

    public class NuevoMensajeViewModel
    {
        public string Asunto { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public List<Usuario> Administradores { get; set; } = new List<Usuario>();
        public int? UsuarioDestinoId { get; set; }
    }

}