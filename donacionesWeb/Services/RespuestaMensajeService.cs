using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class RespuestaMensajeService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/RespuestasMensajes";

        public RespuestaMensajeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RespuestasMensaje>> GetAllAsync()  // Cambiado de GetAllWithDetailsAsync
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<RespuestasMensaje>>() ?? new List<RespuestasMensaje>();
        }

        public async Task<RespuestasMensaje> CreateAsync(RespuestasMensaje respuesta)
        {
            var json = JsonSerializer.Serialize(respuesta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RespuestasMensaje>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
