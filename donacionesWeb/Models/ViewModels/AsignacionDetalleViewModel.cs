namespace donacionesWeb.Models.ViewModels
{
    public class AsignacionDetalleViewModel
    {
        public Asignacione Asignacion { get; set; }
        public decimal MontoAsignado { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string Descripcion => Asignacion?.Descripcion ?? string.Empty;
    }
}
