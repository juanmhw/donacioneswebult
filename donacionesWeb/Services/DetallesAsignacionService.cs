using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class DetallesAsignacionService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/DetallesAsignacions";

        public DetallesAsignacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DetallesAsignacion>> GetDetallesAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DetallesAsignacion>>() ?? new List<DetallesAsignacion>();
        }

        public async Task<DetallesAsignacion> CreateDetalleAsync(DetallesAsignacion detalle)
        {
            var json = JsonSerializer.Serialize(detalle);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DetallesAsignacion>() ?? throw new Exception("Error al deserializar detalle");
        }

        public async Task<bool> DeleteDetalleAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
