using donacionesWeb.Controllers;
using donacionesWeb.Services;
using donacionesWeb.Services.Firebase;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using QuestPDF.Infrastructure;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1) Cargar variables de entorno (APIs, CORS, etc.)
builder.Configuration.AddEnvironmentVariables();

// 2) MVC + JSON
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 3) Cookie Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// 4) CORS (multi-origen vía FRONTEND_ORIGINS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", p =>
    {
        var raw = builder.Configuration["FRONTEND_ORIGINS"] ?? "http://localhost:5173,http://localhost:8080";
        var origins = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        p.WithOrigins(origins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

// 5) HttpClients NOMBRADOS (sin hardcodear localhost)
builder.Services.AddHttpClient("SqlApi", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Sql"] ?? cfg["DONACIONES_API_BASE_URL"]
        ?? throw new InvalidOperationException("Base URL SQL no configurada (Apis:Sql o DONACIONES_API_BASE_URL).");
    client.BaseAddress = new Uri(baseUrl); // ej: http://donaciones-api:8080/api/
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("MongoApi", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Mongo"] ?? cfg["MONGO_API_BASE_URL"]
        ?? throw new InvalidOperationException("Base URL Mongo no configurada (Apis:Mongo o MONGO_API_BASE_URL).");
    client.BaseAddress = new Uri(baseUrl); // ej: http://api-donaciones-mongo:8080/api/
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// 6) DataProtection (usa volumen /var/www/dp-keys)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/www/dp-keys"))
    .SetApplicationName("DonacionesWeb");

// 7) Registrar tus servicios (SCOPED). NO uses AddHttpClient<T> aquí.
builder.Services.AddScoped<CampaniaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<MensajeService>();
builder.Services.AddScoped<SaldosDonacionService>();
builder.Services.AddScoped<RespuestaMensajeService>();
builder.Services.AddScoped<EstadoService>();
builder.Services.AddScoped<DonacionService>();
builder.Services.AddScoped<DonacionAsignacionService>();
builder.Services.AddScoped<DetallesAsignacionService>();
builder.Services.AddScoped<AsignacionService>();
builder.Services.AddScoped<RendicionCuentasController>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<UsuarioRolService>();
builder.Services.AddScoped<RolService>();

// Extras
builder.Services.AddSingleton<FirebaseStorageService>();
builder.Services.AddHttpClient<SupabaseStorageService>();

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// 8) Proxy reverso (si usas Nginx delante)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseCors("PermitirFrontend");

// Si usas solo HTTP dentro del contenedor, puedes comentar la siguiente
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Health endpoint para Docker/Nginx
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();

// === Conversor global ===
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.Parse(reader.GetString());
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}
