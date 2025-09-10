namespace donacionesWeb.Models
{
    public class SaldosDonacione
    {
        public int SaldoId { get; set; }
        public int DonacionId { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal? MontoUtilizado { get; set; }
        public decimal SaldoDisponible { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
    }
}
