
using Blazored.LocalStorage;
using BlazorWebAppCustomer;
using BlazorWebAppCustomer.Data;
using BlazorWebAppCustomer.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// ===== Add services =====
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


// Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();


////////////
//// ApiSettings
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// HttpClient với BaseAddress động
//builder.Services.AddHttpClient<IUserCustomerService, UserCustomerService>((sp, client) =>
//{
//    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
//    client.BaseAddress = new Uri(settings.BaseUrl);
//    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
//});

//builder.Services.AddHttpClient<MovieService>((sp, client) =>
//{
//    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
//    client.BaseAddress = new Uri(settings.BaseUrl);
//    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
//});

builder.Services.AddHttpClient<IUserCustomerService, UserCustomerService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});

builder.Services.AddHttpClient<MovieService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});

builder.Services.AddHttpClient<ApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    Console.WriteLine($"[DEBUG] API BaseUrl: {settings.BaseUrl}");
});




//builder.Services.AddScoped<IUserCustomerService, UserCustomerService>();

//builder.Services.AddHttpClient("GateWayApiProduct", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7076/product-service/api/");
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();