using System.Net.Http.Headers;

namespace donacionesWeb.Services
{
    public class SupabaseStorageService
    {
        private readonly HttpClient _httpClient;
        private const string SupabaseUrl = "https://tjuafoiemlxssyyfhden.supabase.co";
        private const string SupabaseBucket = "transparencia-bucket";
        private const string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRqdWFmb2llbWx4c3N5eWZoZGVuIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTAxNzY2ODQsImV4cCI6MjA2NTc1MjY4NH0.JyqTwqBrEoNhJ-SOOnIqdhrRoEFD7k39etkV9qQysks";

        public SupabaseStorageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseAnonKey);
            _httpClient.DefaultRequestHeaders.Add("apikey", SupabaseAnonKey);
        }

        public async Task<string?> SubirImagenAsync(IFormFile archivo, string carpeta)
        {
            var nombreArchivo = $"{carpeta}/{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var url = $"{SupabaseUrl}/storage/v1/object/{SupabaseBucket}/{nombreArchivo}";

            using var stream = archivo.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode) return null;

            return $"{SupabaseUrl}/storage/v1/object/public/{SupabaseBucket}/{nombreArchivo}";
        }
    }
}
