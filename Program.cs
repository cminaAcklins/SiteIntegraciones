using Integraciones.Services;

var builder = WebApplication.CreateBuilder(args);

// Habilitar logging a consola y archivos
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Habilitar que los logs de stdout se creen en producción
builder.WebHost.CaptureStartupErrors(true)
               .UseSetting("detailedErrors", "true");

// Add services to the container
builder.Services.AddControllersWithViews();

// ApiService con HttpClient usando appsettings
builder.Services.AddHttpContextAccessor();
var apiBase = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://catastroapiack-fcfwhuf8cwhkd9gj.westus-01.azurewebsites.net/api/"; 
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});

// Configuración de Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Middleware de session (antes de routing y auth)
app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🚫 Ya no usamos Authentication/Authorization de cookies
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
