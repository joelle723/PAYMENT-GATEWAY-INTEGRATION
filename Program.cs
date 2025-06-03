using Stripe;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews(); // Enables MVC with Views
builder.Services.AddControllers();          // Enables attribute-based controllers like [Route("stripe-webhook")]

// Stripe configuration from appsettings.json
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Debug log to confirm PublishableKey is loaded correctly
var publishableKeyAtStartup = builder.Configuration["Stripe:PublishableKey"];
Console.WriteLine($"DEBUG (Program.cs): PublishableKey at startup: '{publishableKeyAtStartup}'");

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Route for traditional controllers (like HomeController)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ? Route for attribute-routed controllers like WebhookController
app.MapControllers();

app.Run();
