using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class RolService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://apidonacionesbeni.somee.com/api/Roles";

        public RolService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Rol>> GetRolesAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Rol>>() ?? new List<Rol>();
        }

        public async Task<Rol> GetRolByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Rol>() ?? throw new Exception("Error al deserializar rol");
        }

        public async Task<Rol> CreateRolAsync(Rol rol)
        {
            var json = JsonSerializer.Serialize(rol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Rol>() ?? throw new Exception("Error al deserializar rol");
        }

        public async Task<Rol> UpdateRolAsync(int id, Rol rol)
        {
            var json = JsonSerializer.Serialize(rol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{id}", content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Rol>() ?? throw new Exception("Error al deserializar rol");
        }

        public async Task<bool> DeleteRolAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}