using Tacit.Brand.Sample;
using Tacit.Brand.Sample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
// Dane w pamięci, wspólne dla stron statycznych i interaktywnych. Znikają po restarcie.
builder.Services.AddSingleton<ClientStore>();

var app = builder.Build();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
