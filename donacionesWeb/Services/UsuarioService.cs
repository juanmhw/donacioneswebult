using donacionesWeb.Models;
using System.Text.Json;
using System.Text;

namespace donacionesWeb.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _http;

        // Usamos la factory para obtener el cliente nombrado "SqlApi"
        public UsuarioService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi"); // BaseAddress termina en /api/
        }

        // GET /api/Usuarios
        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var res = await _http.GetAsync("Usuarios");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Usuario>>() ?? new List<Usuario>();
        }

        // GET /api/Usuarios/{id}
        public async Task<Usuario> GetUsuarioByIdAsync(int id)
        {
            var res = await _http.GetAsync($"Usuarios/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Usuario>()
                   ?? throw new Exception("Error al deserializar usuario");
        }

        // POST /api/Usuarios
        public async Task<Usuario> CreateUsuarioAsync(Usuario usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("Usuarios", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Usuario>()
                   ?? throw new Exception("Error al deserializar usuario");
        }

        // PUT /api/Usuarios/{id}
        public async Task<Usuario> UpdateUsuarioAsync(int id, Usuario usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _http.PutAsync($"Usuarios/{id}", content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Usuario>()
                   ?? throw new Exception("Error al deserializar usuario");
        }

        // DELETE /api/Usuarios/{id}
        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            var res = await _http.DeleteAsync($"Usuarios/{id}");
            return res.IsSuccessStatusCode;
        }

        // (Si tu API lo expone) GET /api/Usuarios/email/{email}
        public async Task<Usuario?> GetUsuarioByEmailAsync(string email)
        {
            var res = await _http.GetAsync($"Usuarios/email/{email}");
            if (!res.IsSuccessStatusCode) return null;

            var content = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Usuario>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
