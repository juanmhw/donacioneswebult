using donacionesWeb.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class DetallesAsignacionViewModel
{
    [ValidateNever]
    public Asignacione Asignacion { get; set; } = new();

    public DetallesAsignacion NuevoDetalle { get; set; } = new();

    public List<DetallesAsignacion> Detalles { get; set; } = new();

    public List<DonacionesAsignacione> DonacionesAsignadas { get; set; } = new();

    public decimal TotalAsignado => Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
    public IFormFile? Imagen { get; set; }
}

