using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace donacionesWeb.Services
{
    public class DonacionAsignacionService
    {
        private readonly HttpClient _httpClient;

        public DonacionAsignacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5097/api/"); // ✅ BaseAddress obligatoria
        }

        // Obtener todas las asignaciones de donaciones
        public async Task<List<DonacionesAsignacione>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("DonacionesAsignaciones");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<DonacionesAsignacione>>() ?? new List<DonacionesAsignacione>();
        }

        // Crear nueva asignación de donación
        public async Task CreateAsync(DonacionesAsignacione model)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(model, options);
            Console.WriteLine("📤 Enviando DonacionAsignacion:");
            Console.WriteLine(json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("DonacionesAsignaciones", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error al guardar: {response.StatusCode} - {errorContent}");
                throw new Exception($"Error al guardar la asignación de donación: {errorContent}");
            }

            Console.WriteLine("✅ Donación asignada correctamente en la API.");
        }

        // Eliminar asignación (opcional)
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"DonacionesAsignaciones/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
