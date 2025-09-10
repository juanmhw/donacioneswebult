namespace donacionesWeb.Models
{
    public class Estado
    {
        public int EstadoId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
    }
}
