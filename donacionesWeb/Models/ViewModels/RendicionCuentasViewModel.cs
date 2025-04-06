namespace donacionesWeb.Models.ViewModels
{
    public class RendicionCuentasViewModel
    {
        public List<Donacione> MisDonaciones { get; set; } = new List<Donacione>();
        public List<DonacionConAsignacionesViewModel> DonacionesConDestino { get; set; } = new List<DonacionConAsignacionesViewModel>();
        public decimal TotalDonado { get; set; }
        public int CampaniasApoyadas { get; set; }
    }

    public class DonacionConAsignacionesViewModel
    {
        public Donacione Donacion { get; set; }
        public List<AsignacionDetalleViewModel> Asignaciones { get; set; } = new List<AsignacionDetalleViewModel>();
        public decimal PorcentajeUtilizado => Donacion.Monto > 0 ?
            (Asignaciones.Sum(a => a.MontoAsignado) / Donacion.Monto) * 100 : 0;
    }

    public class AsignacionDetalleViewModel
    {
        public Asignacione Asignacion { get; set; }
        public decimal MontoAsignado { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string Descripcion => Asignacion?.Descripcion ?? string.Empty;

    }
}
