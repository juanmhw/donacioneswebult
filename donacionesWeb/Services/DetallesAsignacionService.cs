using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class DetallesAsignacionService
    {
        private readonly HttpClient _httpClient;

        public DetallesAsignacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://apidonacionesbeni.somee.com/api/");
        }

        // Obtener todos los detalles
        public async Task<List<DetallesAsignacion>> GetDetallesAsync()
        {
            var response = await _httpClient.GetAsync("DetallesAsignacions");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DetallesAsignacion>>() ?? new List<DetallesAsignacion>();
        }

        // Obtener detalles por asignación específica
        public async Task<List<DetallesAsignacion>> GetByAsignacionAsync(int asignacionId)
        {
            var response = await _httpClient.GetAsync($"DetallesAsignacions/asignacion/{asignacionId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DetallesAsignacion>>() ?? new List<DetallesAsignacion>();
        }

        // Crear nuevo detalle con log
        public async Task CreateDetalleAsync(DetallesAsignacion detalle)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(detalle, options);
            Console.WriteLine("📤 [POST] Enviando detalle a la API:");
            Console.WriteLine(json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("DetallesAsignacions", content);

            Console.WriteLine($"📥 Respuesta de la API: {(int)response.StatusCode} - {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error al crear detalle: {errorContent}");
                throw new Exception($"Error al crear el detalle: {errorContent}");
            }

            Console.WriteLine("✅ Detalle creado correctamente.");
        }

        // Eliminar detalle por ID
        public async Task<bool> DeleteDetalleAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"DetallesAsignacions/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
