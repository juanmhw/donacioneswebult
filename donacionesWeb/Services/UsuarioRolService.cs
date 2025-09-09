using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class UsuarioRolService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://apidonacionesbeni.somee.com/api/UsuariosRoles";

        public UsuarioRolService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UsuarioRol>> GetUsuariosRolesAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<UsuarioRol>>() ?? new List<UsuarioRol>();
        }

        public async Task<UsuarioRol> GetUsuarioRolByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UsuarioRol>() ?? throw new Exception("Error al deserializar usuario rol");
        }

        public async Task<UsuarioRol> CreateUsuarioRolAsync(UsuarioRol usuarioRol)
        {
            var json = JsonSerializer.Serialize(usuarioRol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UsuarioRol>() ?? throw new Exception("Error al deserializar usuario rol");
        }

        public async Task<UsuarioRol> UpdateUsuarioRolAsync(int id, UsuarioRol usuarioRol)
        {
            var json = JsonSerializer.Serialize(usuarioRol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UsuarioRol>() ?? throw new Exception("Error al deserializar usuario rol");
        }

        public async Task<bool> DeleteUsuarioRolAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<UsuarioRol>> GetUsuariosRolesByUsuarioIdAsync(int usuarioId)
        {
            // Necesitas agregar un endpoint en tu API que devuelva roles por usuarioId
            var response = await _httpClient.GetAsync($"{BaseUrl}/usuario/{usuarioId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<UsuarioRol>>() ?? new List<UsuarioRol>();
        }
    }
}