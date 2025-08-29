using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;
using FrontendProyecto.Infrastructure; // donde está JwtSessionHandler
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// ---------- Razor Pages ----------
builder.Services.AddRazorPages();

// ---------- Auth (Cookies) ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// ---------- Authorization (UNA sola vez, ANTES de Build) ----------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageOngs", policy =>
        policy.RequireRole("Administrador", "Coordinador"));
    // si quieres, aquí agregas más policies (Proyectos, Actividades, etc.)
});

// ---------- HttpClient + Handler JWT ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtSessionHandler>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7233"); // <-- tu backend
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<JwtSessionHandler>();

// ---------- Session ----------
builder.Services.AddSession();

var app = builder.Build();

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization(); // <- FALTABA

app.MapRazorPages();
app.Run();
