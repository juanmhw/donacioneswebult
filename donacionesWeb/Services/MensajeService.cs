using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class MensajeService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/Mensajes";

        public MensajeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Mensaje>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Mensaje>>() ?? new List<Mensaje>();
        }

        // Agrega este nuevo método al servicio
        public async Task<Mensaje> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Mensaje>();
        }

        public async Task<Mensaje> CreateAsync(Mensaje mensaje)
        {
            var json = JsonSerializer.Serialize(mensaje);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Mensaje>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
