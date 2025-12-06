using SchoolManagementSystem.Web.Client.Pages;
using SchoolManagementSystem.Web.Components;
using SchoolManagementSystem.Web.Data;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
    
builder.Services.AddScoped<SchoolService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SchoolManagementSystem.Web.Client._Imports).Assembly);

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<SchoolService>();
    // Assuming the file is in the parent directory of the solution or hardcoded for now suitable for the user's workspace
    string jsonPath = "/Users/antoanpeychev/Projects/daskAL/school_data.json"; 
    await service.ImportFromLegacyJsonAsync(jsonPath);
}

app.Run();
