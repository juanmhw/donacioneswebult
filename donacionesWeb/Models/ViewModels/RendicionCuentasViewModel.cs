namespace donacionesWeb.Models.ViewModels
{
    public class RendicionCuentasViewModel
    {
        public RendicionCuentasViewModel()
        {
            MisDonaciones = new List<Donacione>();
            DonacionesConDestino = new List<DonacionConAsignacionesViewModel>();
        }

        public List<Donacione> MisDonaciones { get; set; }
        public List<DonacionConAsignacionesViewModel> DonacionesConDestino { get; set; }
        public decimal TotalDonado { get; set; }
        public int CampaniasApoyadas { get; set; }
        public int TotalDonacionesCompletadas { get; set; }
        public DateTime? UltimaDonacion { get; set; }
        public decimal PromedioMontoDonado { get; set; }
        public decimal DonacionMasAlta { get; set; }
        public decimal TotalAsignado { get; set; }
        public decimal TotalPendienteAsignacion { get; set; }
    }

    public class DonacionConAsignacionesViewModel
    {
        public DonacionConAsignacionesViewModel()
        {
            Asignaciones = new List<AsignacionDetalleViewModel>();
        }

        public Donacione Donacion { get; set; }
        public Campania Campania { get; set; }
        public SaldosDonacione SaldoDonacion { get; set; }
        public List<AsignacionDetalleViewModel> Asignaciones { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal MontoUtilizado { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal PorcentajeUtilizado { get; set; }
    }

    public class AsignacionDetalleViewModel
    {
        public AsignacionDetalleViewModel()
        {
            DetallesAsignacion = new List<DetallesAsignacion>();
        }

        public Asignacione Asignacion { get; set; }
        public decimal MontoAsignado { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public List<DetallesAsignacion> DetallesAsignacion { get; set; }
        public string ComprobanteUrl { get; set; }
    }

    public class DetalleDonacionViewModel
    {
        public DetalleDonacionViewModel()
        {
            Asignaciones = new List<AsignacionDetalleViewModel>();
        }

        public Donacione Donacion { get; set; }
        public Campania Campania { get; set; }
        public SaldosDonacione SaldoDonacion { get; set; }
        public List<AsignacionDetalleViewModel> Asignaciones { get; set; }
    }
}