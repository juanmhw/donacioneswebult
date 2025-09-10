using donacionesWeb.Models;
using donacionesWeb.Models.ViewModels;
using Xunit;
using System.Collections.Generic;

namespace donacionesWeb.Tests.ViewModels
{
    public class DetallesAsignacionViewModelTests
    {
        [Fact]
        public void TotalAsignado_DeberiaCalcularCorrectamente()
        {
            var vm = new DetallesAsignacionViewModel
            {
                Detalles = new List<DetallesAsignacion>
                {
                    new DetallesAsignacion { Cantidad = 2, PrecioUnitario = 50 },
                    new DetallesAsignacion { Cantidad = 1, PrecioUnitario = 100 }
                }
            };

            Assert.Equal(200, vm.TotalAsignado);
        }
    }
}
