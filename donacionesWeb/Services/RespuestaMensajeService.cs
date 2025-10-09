using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class RespuestaMensajeService
    {
        private readonly HttpClient _http;
        private readonly MensajeService _mensajeService;

        // Pedimos la factory y construimos el cliente "SqlApi"
        public RespuestaMensajeService(IHttpClientFactory factory, MensajeService mensajeService)
        {
            _http = factory.CreateClient("SqlApi");
            _mensajeService = mensajeService;
        }

        // GET /api/RespuestasMensajes
        public async Task<List<RespuestaMensaje>> GetRespuestasAsync()
        {
            var res = await _http.GetAsync("RespuestasMensajes");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<RespuestaMensaje>>() ?? new List<RespuestaMensaje>();
        }

        // GET /api/RespuestasMensajes/{id}
        public async Task<RespuestaMensaje> GetRespuestaByIdAsync(int id)
        {
            var res = await _http.GetAsync($"RespuestasMensajes/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<RespuestaMensaje>()
                   ?? throw new Exception("Error al deserializar respuesta");
        }

        // GET /api/RespuestasMensajes/mensaje/{mensajeId}  (si existe en tu API)
        public async Task<List<RespuestaMensaje>> GetRespuestasByMensajeIdAsync(int mensajeId)
        {
            var res = await _http.GetAsync($"RespuestasMensajes/mensaje/{mensajeId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<RespuestaMensaje>>() ?? new List<RespuestaMensaje>();
        }

        // POST /api/RespuestasMensajes
        public async Task<RespuestaMensaje> CreateRespuestaAsync(RespuestaMensaje respuesta)
        {
            var json = JsonSerializer.Serialize(respuesta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("RespuestasMensajes", content);
            res.EnsureSuccessStatusCode();

            // Marcar el mensaje como respondido
            await _mensajeService.MarcarComoRespondidoAsync(respuesta.MensajeId);

            return await res.Content.ReadFromJsonAsync<RespuestaMensaje>()
                   ?? throw new Exception("Error al deserializar respuesta");
        }

        // PUT /api/RespuestasMensajes/{id}
        public async Task<RespuestaMensaje> UpdateRespuestaAsync(int id, RespuestaMensaje respuesta)
        {
            var json = JsonSerializer.Serialize(respuesta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PutAsync($"RespuestasMensajes/{id}", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<RespuestaMensaje>()
                   ?? throw new Exception("Error al deserializar respuesta");
        }

        // DELETE /api/RespuestasMensajes/{id}
        public async Task<bool> DeleteRespuestaAsync(int id)
        {
            var res = await _http.DeleteAsync($"RespuestasMensajes/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}
