namespace donacionesWeb.Models.ViewModels
{
    public class RendicionCuentasViewModel
    {
        public List<Donacione> MisDonaciones { get; set; } = new List<Donacione>();
        public List<DonacionConAsignacionesViewModel> DonacionesConDestino { get; set; } = new List<DonacionConAsignacionesViewModel>();
        public decimal TotalDonado { get; set; }
        public int CampaniasApoyadas { get; set; }
    }
}
