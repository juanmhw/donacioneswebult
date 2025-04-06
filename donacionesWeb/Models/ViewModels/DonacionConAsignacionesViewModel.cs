namespace donacionesWeb.Models.ViewModels
{
    public class DonacionConAsignacionesViewModel
    {
        public Donacione Donacion { get; set; }
        public List<AsignacionDetalleViewModel> Asignaciones { get; set; } = new List<AsignacionDetalleViewModel>();
        public decimal PorcentajeUtilizado => Donacion.Monto > 0 ?
            (Asignaciones.Sum(a => a.MontoAsignado) / Donacion.Monto) * 100 : 0;
    }
}
