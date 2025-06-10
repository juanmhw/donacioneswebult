namespace donacionesWeb.Models.ViewModels
{
    public class AsignacionReporteViewModel
    {
        public int AsignacionId { get; set; }
        public string Descripcion { get; set; }
        public string Campania { get; set; }
        public DateTime Fecha { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoAsignado { get; set; }
        public List<string> Detalles { get; set; } = new();
    }

}
