using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class RespuestaMensajeService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://apidonacionesbeni.somee.com/api/RespuestasMensajes";
        private readonly MensajeService _mensajeService;

        public RespuestaMensajeService(HttpClient httpClient, MensajeService mensajeService)
        {
            _httpClient = httpClient;
            _mensajeService = mensajeService;
        }

        public async Task<List<RespuestaMensaje>> GetRespuestasAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<RespuestaMensaje>>() ?? new List<RespuestaMensaje>();
        }

        public async Task<RespuestaMensaje> GetRespuestaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RespuestaMensaje>() ?? throw new Exception("Error al deserializar respuesta");
        }

        public async Task<List<RespuestaMensaje>> GetRespuestasByMensajeIdAsync(int mensajeId)
        {
            // Asumiendo que has implementado este endpoint en tu API
            var response = await _httpClient.GetAsync($"{BaseUrl}/mensaje/{mensajeId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<RespuestaMensaje>>() ?? new List<RespuestaMensaje>();
        }

        public async Task<RespuestaMensaje> CreateRespuestaAsync(RespuestaMensaje respuesta)
        {
            var json = JsonSerializer.Serialize(respuesta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();

            // Marcar el mensaje como respondido
            await _mensajeService.MarcarComoRespondidoAsync(respuesta.MensajeId);

            return await response.Content.ReadFromJsonAsync<RespuestaMensaje>() ?? throw new Exception("Error al deserializar respuesta");
        }

        public async Task<RespuestaMensaje> UpdateRespuestaAsync(int id, RespuestaMensaje respuesta)
        {
            var json = JsonSerializer.Serialize(respuesta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RespuestaMensaje>() ?? throw new Exception("Error al deserializar respuesta");
        }

        public async Task<bool> DeleteRespuestaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}