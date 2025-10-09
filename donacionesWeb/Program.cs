using donacionesWeb.Controllers;
using donacionesWeb.Services;
using donacionesWeb.Services.Firebase;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuestPDF.Infrastructure;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1) Variables de entorno (APIs, CORS, etc.)
builder.Configuration.AddEnvironmentVariables();

// 2) MVC + JSON + Antiforgery global
builder.Services.AddControllersWithViews(options =>
{
    // OWASP: auto-validar CSRF en toda acción modificadora (POST/PUT/PATCH/DELETE) de MVC
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// 2.1) Encoder seguro (evita XSS al renderizar texto en vistas)
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement));

// 3) Cookie Auth endurecida
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        // OWASP: cookies seguras
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = "DonacionesWeb.Auth";
    });

// 3.1) CSRF para formularios/cookies (si usas SPA, envía X-CSRF-TOKEN en requests)
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN"; // útil para SPA
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.SuppressXFrameOptionsHeader = true; // ya lo seteamos nosotros
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
         .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
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

// 7) Registrar servicios (SCOPED)
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

// 8) Seguridad transversal: HTTPS, HSTS y Rate Limiting
builder.Services.AddHttpsRedirection(o =>
{
    o.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    o.HttpsPort = 443;
});

builder.Services.AddHsts(o =>
{
    o.Preload = true;
    o.IncludeSubDomains = true;
    o.MaxAge = TimeSpan.FromDays(180);
});

// Rate limiting global por IP (anti-abuso)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var key = ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120, // 120 req/min por IP
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();
app.Use((ctx, next) => { if (Environment.GetEnvironmentVariable("FORCE_HTTPS_SCHEME") == "true") ctx.Request.Scheme = "https"; return next(); });

// 9) Proxy reverso (si usas Nginx delante)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 10) Manejo de errores y HSTS
if (!app.Environment.IsDevelopment())
{
    // OWASP: error handler seguro (no expone stacktrace)
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 11) Redirección a HTTPS
app.UseHttpsRedirection();

// 12) Cabeceras de seguridad (CSP, XFO, XCTO, RP, Permissions-Policy)
app.Use(async (ctx, next) =>
{
    // Ajusta CSP según tus necesidades (scripts externos, CDNs, etc.)
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "img-src 'self' data: https:; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " + // permite CSS inline si lo necesitas
        "frame-ancestors 'none';";

    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

    await next();
});

// 13) CORS, estáticos, routing, auth, rate limiting
app.UseCors("PermitirFrontend");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// 14) Health endpoint para Docker/Nginx
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// 15) Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();

// === Conversor global ===
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.Parse(reader.GetString()!);
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}
