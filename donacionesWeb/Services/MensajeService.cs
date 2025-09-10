using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class MensajeService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://apidonacionesbeni.somee.com/api/Mensajes";

        public MensajeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Mensaje>> GetMensajesAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Mensaje>>() ?? new List<Mensaje>();
        }

        public async Task<Mensaje> GetMensajeByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Mensaje>() ?? throw new Exception("Error al deserializar mensaje");
        }

        public async Task<List<Mensaje>> GetMensajesByUsuarioOrigenAsync(int usuarioId)
        {
            // Asumiendo que has implementado este endpoint en tu API
            var response = await _httpClient.GetAsync($"{BaseUrl}/usuario-origen/{usuarioId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Mensaje>>() ?? new List<Mensaje>();
        }

        public async Task<List<Mensaje>> GetMensajesByUsuarioDestinoAsync(int usuarioId)
        {
            // Asumiendo que has implementado este endpoint en tu API
            var response = await _httpClient.GetAsync($"{BaseUrl}/usuario-destino/{usuarioId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Mensaje>>() ?? new List<Mensaje>();
        }

        public async Task<Mensaje> CreateMensajeAsync(Mensaje mensaje)
        {
            var json = JsonSerializer.Serialize(mensaje);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Mensaje>() ?? throw new Exception("Error al deserializar mensaje");
        }

        public async Task<Mensaje> UpdateMensajeAsync(int id, Mensaje mensaje)
        {
            var json = JsonSerializer.Serialize(mensaje);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Mensaje>() ?? throw new Exception("Error al deserializar mensaje");
        }

        public async Task<bool> DeleteMensajeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarcarComoLeidoAsync(int id)
        {
            try
            {
                var mensaje = await GetMensajeByIdAsync(id);
                mensaje.Leido = true;
                await UpdateMensajeAsync(id, mensaje);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarcarComoRespondidoAsync(int id)
        {
            try
            {
                var mensaje = await GetMensajeByIdAsync(id);
                mensaje.Respondido = true;
                await UpdateMensajeAsync(id, mensaje);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}