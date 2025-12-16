using System.Text;
using ASECCC_Digital.Services;

var builder = WebApplication.CreateBuilder(args);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddControllersWithViews();


builder.Services.AddHttpClient<IBeneficiosServiciosClient, BeneficiosServiciosClient>(client =>
{
    var baseUrl = builder.Configuration["Valores:UrlAPI"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Falta configurar 'Valores:UrlAPI' en appsettings.json");

    client.BaseAddress = new Uri(baseUrl);
});


builder.Services.AddHttpClient();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseExceptionHandler("/Error/MostrarError");

app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles(); 

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
