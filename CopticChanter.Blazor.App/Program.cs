using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CopticChanter.Blazor.App;
using CopticChanter.WebApi.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
builder.Services.AddSingleton(sp => new CoptClient("https://localhost:7257"));

// Initialize CoptLib
CoptLib.Scripting.DotNetScript.Register();
CoptLib.Scripting.LuaScript.Register();
builder.Services.AddSingleton<CoptLib.IO.ILoadContext>(new CoptLib.IO.LoadContext());

await builder.Build().RunAsync();