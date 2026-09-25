using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Shos.Minesweeper;
using Shos.Minesweeper.Browser;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<BrowserFeatures>();
builder.Services.AddScoped<BestTimeStorage>();

await builder.Build().RunAsync();
