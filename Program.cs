using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Business.Data;

var builder = WebApplication.CreateBuilder(args);

// Configura il database MySQL

var connectionString = builder.Configuration.GetConnectionString("MySqlConn");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configura ASP.NET Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configura il logging (opzionale, ma utile per il debug)
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddFilter(level => level >= LogLevel.Information);
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); 
builder.Services.AddScoped<TripController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

builder.WebHost.UseStaticWebAssets();

app.UseHttpsRedirection();
app.UseStaticFiles(); // Abilita i file statici
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();