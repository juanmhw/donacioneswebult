public class DonacionDetalleViewModel
{
    public string Nombre { get; set; } = "Anónimo";
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public decimal Asignado { get; set; }

    // ✅ No pongas set aquí, porque Saldo se calcula automáticamente
    public decimal Saldo => Monto - Asignado;

    public List<ItemAsignadoViewModel> Asignaciones { get; set; } = new();
}
