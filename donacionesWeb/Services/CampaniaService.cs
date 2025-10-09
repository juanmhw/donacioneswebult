using donacionesWeb.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace donacionesWeb.Services
{
    public class CampaniaService
    {
        private readonly HttpClient _http;

        public CampaniaService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SqlApi");
            // BaseAddress ya configurada en Program.cs → termina en /api/
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new DateOnlyJsonConverter() }
        };

        // GET /api/Campanias
        public async Task<IEnumerable<Campania>> GetAllAsync(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<IEnumerable<Campania>>("Campanias", _jsonOptions, ct)
                   ?? Enumerable.Empty<Campania>();
        }

        // GET /api/Campanias/{id}
        public async Task<Campania?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<Campania>($"Campanias/{id}", _jsonOptions, ct);
        }

        // POST /api/Campanias
        public async Task CreateAsync(Campania campania, CancellationToken ct = default)
        {
            var campaniaApi = new
            {
                titulo = campania.Titulo,
                descripcion = campania.Descripcion,
                fechaInicio = DateOnly.FromDateTime(campania.FechaInicio),
                fechaFin = campania.FechaFin.HasValue ? DateOnly.FromDateTime(campania.FechaFin.Value) : (DateOnly?)null,
                metaRecaudacion = campania.MetaRecaudacion,
                montoRecaudado = campania.MontoRecaudado ?? 0,
                usuarioIdcreador = campania.UsuarioIdcreador,
                activa = campania.Activa ?? true,
                fechaCreacion = DateTime.Now,
                imagenUrl = campania.ImagenUrl
            };

            var res = await _http.PostAsJsonAsync("Campanias", campaniaApi, _jsonOptions, ct);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Error API: {res.StatusCode} - {error}");
            }
        }

        // PUT /api/Campanias/{id}
        public async Task UpdateAsync(int id, Campania campania, CancellationToken ct = default)
        {
            var campaniaApi = new
            {
                titulo = campania.Titulo,
                descripcion = campania.Descripcion,
                fechaInicio = DateOnly.FromDateTime(campania.FechaInicio),
                fechaFin = campania.FechaFin.HasValue ? DateOnly.FromDateTime(campania.FechaFin.Value) : (DateOnly?)null,
                metaRecaudacion = campania.MetaRecaudacion,
                montoRecaudado = campania.MontoRecaudado ?? 0,
                usuarioIdcreador = campania.UsuarioIdcreador,
                activa = campania.Activa ?? true,
                fechaCreacion = campania.FechaCreacion,
                imagenUrl = campania.ImagenUrl
            };

            var res = await _http.PutAsJsonAsync($"Campanias/{id}", campaniaApi, _jsonOptions, ct);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Error API: {res.StatusCode} - {error}");
            }
        }

        // DELETE /api/Campanias/{id}
        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var res = await _http.DeleteAsync($"Campanias/{id}", ct);
            res.EnsureSuccessStatusCode();
        }

        // Conversor para DateOnly
        public class DateOnlyJsonConverter : JsonConverter<DateOnly>
        {
            private const string Format = "yyyy-MM-dd";

            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => DateOnly.Parse(reader.GetString()!);

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString(Format));
        }
    }
}
