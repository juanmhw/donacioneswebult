namespace donacionesWeb.Models
{
    public class DonacionesAsignacione
    {
        public int DonacionAsignacionId { get; set; }
        public int DonacionId { get; set; }
        public int AsignacionId { get; set; }
        public decimal MontoAsignado { get; set; }
        public DateTime? FechaAsignacion { get; set; }
    }
}
