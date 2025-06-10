public class CierreCajaViewModel
{
    public string Campania { get; set; }
    public decimal MontoRecaudado { get; set; }
    public decimal MontoDonado { get; set; }
    public decimal MontoAsignado { get; set; }
    public decimal SaldoRestante => MontoDonado - MontoAsignado;

    public List<string> Donantes { get; set; } = new();
    public List<ItemAsignadoViewModel> DetallesAsignaciones { get; set; } = new();

    // 👇 Esta es la propiedad que te falta:
    public List<DonacionDetalleViewModel> Donaciones { get; set; } = new();
}


public class ItemAsignadoViewModel
{
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public DateTime Fecha { get; set; }  // ← NECESARIA PARA FILTROS
    public decimal Total => Cantidad * PrecioUnitario;
}

