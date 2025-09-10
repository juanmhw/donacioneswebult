namespace donacionesWeb.Models
{
    public class DetallesAsignacion
    {
        public int DetalleId { get; set; }
        public int AsignacionId { get; set; }
        public string Concepto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? ImagenUrl { get; set; }

    }
}
