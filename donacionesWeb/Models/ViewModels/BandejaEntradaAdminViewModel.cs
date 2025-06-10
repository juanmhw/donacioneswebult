using donacionesWeb.Models;

namespace donacionesWeb.Models.ViewModels
{
    // ViewModel para la vista de conversaciones del admin
    public class BandejaEntradaAdminViewModel
    {
        public List<ConversacionViewModel> Conversaciones { get; set; } = new List<ConversacionViewModel>();
    }

    // ViewModel para cada conversación individual
    public class ConversacionViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public string UltimoMensaje { get; set; } = string.Empty;
        public bool EsLeido { get; set; } = true;
        public List<object> MensajesYRespuestas { get; set; } = new List<object>();
        public DateTime? FechaUltimoMensaje { get; set; }
    }

    // ViewModel para el detalle de conversación completa
    public class ConversacionDetalleViewModel
    {
        public Usuario OtroUsuario { get; set; } = new Usuario();
        public List<object> MensajesYRespuestas { get; set; } = new List<object>();
        public int UsuarioActualId { get; set; }
    }

    // Modificación del ViewModel existente para soportar conversaciones
    public class NuevaRespuestaViewModel
    {
        public int MensajeId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public int? OtroUsuarioId { get; set; } // Para el caso de admin respondiendo en conversación
    }
}