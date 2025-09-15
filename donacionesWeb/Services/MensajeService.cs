using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class MensajeService
    {
        private readonly HttpClient _http;

        // Ahora pedimos la factory y creamos el cliente "SqlApi"
        public MensajeService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi");
        }

        // Endpoints relativos sobre /api/
        // => /api/Mensajes
        public async Task<List<Mensaje>> GetMensajesAsync()
        {
            var res = await _http.GetAsync("Mensajes");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Mensaje>>() ?? new List<Mensaje>();
        }

        // => /api/Mensajes/{id}
        public async Task<Mensaje> GetMensajeByIdAsync(int id)
        {
            var res = await _http.GetAsync($"Mensajes/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Mensaje>()
                   ?? throw new Exception("Error al deserializar mensaje");
        }

        // Si tu API expone estos endpoints:
        // => /api/Mensajes/usuario-origen/{usuarioId}
        public async Task<List<Mensaje>> GetMensajesByUsuarioOrigenAsync(int usuarioId)
        {
            var res = await _http.GetAsync($"Mensajes/usuario-origen/{usuarioId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Mensaje>>() ?? new List<Mensaje>();
        }

        // => /api/Mensajes/usuario-destino/{usuarioId}
        public async Task<List<Mensaje>> GetMensajesByUsuarioDestinoAsync(int usuarioId)
        {
            var res = await _http.GetAsync($"Mensajes/usuario-destino/{usuarioId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Mensaje>>() ?? new List<Mensaje>();
        }

        // => POST /api/Mensajes
        public async Task<Mensaje> CreateMensajeAsync(Mensaje mensaje)
        {
            var json = JsonSerializer.Serialize(mensaje);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("Mensajes", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Mensaje>()
                   ?? throw new Exception("Error al deserializar mensaje");
        }

        // => PUT /api/Mensajes/{id}
        public async Task<Mensaje> UpdateMensajeAsync(int id, Mensaje mensaje)
        {
            var json = JsonSerializer.Serialize(mensaje);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PutAsync($"Mensajes/{id}", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Mensaje>()
                   ?? throw new Exception("Error al deserializar mensaje");
        }

        // => DELETE /api/Mensajes/{id}
        public async Task<bool> DeleteMensajeAsync(int id)
        {
            var res = await _http.DeleteAsync($"Mensajes/{id}");
            return res.IsSuccessStatusCode;
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
            catch { return false; }
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
            catch { return false; }
        }
    }
}
