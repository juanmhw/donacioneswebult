using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class UsuarioRolService
    {
        private readonly HttpClient _http;

        // Usamos la factory para obtener el cliente nombrado "SqlApi"
        public UsuarioRolService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi"); // BaseAddress = .../api/
        }

        // GET /api/UsuariosRoles
        public async Task<List<UsuarioRol>> GetUsuariosRolesAsync()
        {
            var res = await _http.GetAsync("UsuariosRoles");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<UsuarioRol>>() ?? new List<UsuarioRol>();
        }

        // GET /api/UsuariosRoles/{id}
        public async Task<UsuarioRol> GetUsuarioRolByIdAsync(int id)
        {
            var res = await _http.GetAsync($"UsuariosRoles/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<UsuarioRol>()
                   ?? throw new Exception("Error al deserializar usuario rol");
        }

        // POST /api/UsuariosRoles
        public async Task<UsuarioRol> CreateUsuarioRolAsync(UsuarioRol usuarioRol)
        {
            var json = JsonSerializer.Serialize(usuarioRol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("UsuariosRoles", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<UsuarioRol>()
                   ?? throw new Exception("Error al deserializar usuario rol");
        }

        // PUT /api/UsuariosRoles/{id}
        public async Task<UsuarioRol> UpdateUsuarioRolAsync(int id, UsuarioRol usuarioRol)
        {
            var json = JsonSerializer.Serialize(usuarioRol);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PutAsync($"UsuariosRoles/{id}", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<UsuarioRol>()
                   ?? throw new Exception("Error al deserializar usuario rol");
        }

        // DELETE /api/UsuariosRoles/{id}
        public async Task<bool> DeleteUsuarioRolAsync(int id)
        {
            var res = await _http.DeleteAsync($"UsuariosRoles/{id}");
            return res.IsSuccessStatusCode;
        }

        // (Si tu API lo expone) GET /api/UsuariosRoles/usuario/{usuarioId}
        public async Task<List<UsuarioRol>> GetUsuariosRolesByUsuarioIdAsync(int usuarioId)
        {
            var res = await _http.GetAsync($"UsuariosRoles/usuario/{usuarioId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<UsuarioRol>>() ?? new List<UsuarioRol>();
        }
    }
}
