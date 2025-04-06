namespace donacionesWeb.Models
{
    public class Asignacione
    {
        public int AsignacionId { get; set; }
        public int CampaniaId { get; set; }
        public string Descripcion { get; set; } = null!;
        public decimal Monto { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public int UsuarioId { get; set; }
        public string? Comprobante { get; set; }
    }
}
