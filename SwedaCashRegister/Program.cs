using MudBlazor.Services;
using SwedaCashRegister.Components;
using SwedaCashRegister.Components.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<IReceiptTemplateProvider, ReceiptTemplateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

InitConfiguration();

app.Run();

partial class Program
{
    public static string EXECUTING_DIRECTORY { get; set; } = "";

    public static void InitConfiguration()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        EXECUTING_DIRECTORY = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
    }
}