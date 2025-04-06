namespace donacionesWeb.Models
{
    public class Donacione
    {
        public int DonacionId { get; set; }
        public int? UsuarioId { get; set; }
        public int CampaniaId { get; set; }
        public decimal Monto { get; set; }
        public string TipoDonacion { get; set; } = null!;
        public string? Descripcion { get; set; }
        public DateTime? FechaDonacion { get; set; }
        public int EstadoId { get; set; }
        public bool? EsAnonima { get; set; }
    }
}
