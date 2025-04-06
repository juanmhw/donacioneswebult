using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5097/api/Usuarios";

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Usuario>>() ?? new List<Usuario>();
        }

        public async Task<Usuario> CreateUsuarioAsync(Usuario usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Usuario>() ?? throw new Exception("Error al deserializar usuario");
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
