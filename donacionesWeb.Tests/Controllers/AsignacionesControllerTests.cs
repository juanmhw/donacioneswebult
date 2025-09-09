using donacionesWeb.Controllers;
using donacionesWeb.Models;
using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using donacionesWeb.Services.Firebase;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace donacionesWeb.Tests.Controllers
{
    public class AsignacionesControllerTests
    {
        [Fact]
        public async Task Index_DeberiaRetornarVistaConModelo()
        {
            var mockAsignacionService = new Mock<AsignacionService>(null);
            var mockCampaniaService = new Mock<CampaniaService>(null);
            var mockDonacionAsignacionService = new Mock<DonacionAsignacionService>(null);
            var mockFirebaseStorage = new Mock<FirebaseStorageService>(null);

            mockAsignacionService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Asignacione> {
                new Asignacione { AsignacionId = 1, CampaniaId = 1, Descripcion = "Prueba", Monto = 100 }
            });

            mockCampaniaService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Campania> {
                new Campania { CampaniaId = 1, Titulo = "Campaña 1" }
            });

            mockDonacionAsignacionService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<DonacionesAsignacione> {
                new DonacionesAsignacione { AsignacionId = 1, MontoAsignado = 100 }
            });

            var controller = new AsignacionesController(
                mockAsignacionService.Object,
                mockCampaniaService.Object,
                null,
                mockDonacionAsignacionService.Object,
                null,
                null,
                mockFirebaseStorage.Object
            );

            var result = await controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AsignacionIndexViewModel>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Campaña 1", model.First().CampaniaTitulo);
        }

        [Fact]
        public async Task Eliminar_DeberiaRedireccionarADesdeIndex()
        {
            var mockAsignacionService = new Mock<AsignacionService>(null);
            mockAsignacionService.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            var controller = new AsignacionesController(
                mockAsignacionService.Object,
                new Mock<CampaniaService>(null).Object,
                null,
                new Mock<DonacionAsignacionService>(null).Object,
                null,
                null,
                new Mock<FirebaseStorageService>(null).Object
            );

            var result = await controller.Eliminar(1);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task CrearPaso1_ConModeloInvalido_DeberiaRetornarVista()
        {
            var controller = new AsignacionesController(
                new Mock<AsignacionService>(null).Object,
                new Mock<CampaniaService>(null).Object,
                null,
                new Mock<DonacionAsignacionService>(null).Object,
                null,
                null,
                new Mock<FirebaseStorageService>(null).Object
            );

            controller.ModelState.AddModelError("Descripcion", "Campo requerido");
            var modelo = new Asignacione();

            // Aquí pasamos null como segundo parámetro requerido por el método
            var result = await controller.CrearPaso1(modelo, null);

            Assert.IsType<ViewResult>(result);
        }
    }
}
