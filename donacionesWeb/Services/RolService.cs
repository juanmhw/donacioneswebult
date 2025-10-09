using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class RolService
    {
        private readonly HttpClient _http;

        // Usamos factory y el cliente nombrado "SqlApi" (BaseAddress termina en /api/)
        public RolService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi");
        }

        // GET /api/Roles
        public async Task<List<Rol>> GetRolesAsync()
        {
            var res = await _http.GetAsync("Roles");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Rol>>() ?? new List<Rol>();
        }

        // GET /api/Roles/{id}
        public async Task<Rol> GetRolByIdAsync(int id)
        {
            var res = await _http.GetAsync($"Roles/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Rol>()
                   ?? throw new Exception("Error al deserializar rol");
        }

        // POST /api/Roles
        public async Task<Rol> CreateRolAsync(Rol rol)
        {
            var json = JsonSerializer.Serialize(rol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("Roles", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Rol>()
                   ?? throw new Exception("Error al deserializar rol");
        }

        // PUT /api/Roles/{id}
        public async Task<Rol> UpdateRolAsync(int id, Rol rol)
        {
            var json = JsonSerializer.Serialize(rol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PutAsync($"Roles/{id}", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Rol>()
                   ?? throw new Exception("Error al deserializar rol");
        }

        // DELETE /api/Roles/{id}
        public async Task<bool> DeleteRolAsync(int id)
        {
            var res = await _http.DeleteAsync($"Roles/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}
