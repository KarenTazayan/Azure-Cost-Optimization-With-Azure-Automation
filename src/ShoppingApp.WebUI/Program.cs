using Microsoft.ApplicationInsights.Extensibility;
using MudBlazor.Services;
using Orleans.Configuration;
using ShoppingApp.WebUI;
using ShoppingApp.WebUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Azure SignalR for Blazor based WebUI.
/*if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddSignalR().AddAzureSignalR(options =>
    {
        options.ConnectionString = GlobalConfig.AzureSignalRConnection;
        options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required;
    });
}*/

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ShoppingCartService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ComponentStateChangedObserver>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddLocalStorageServices();

// Application Insights.
builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = GlobalConfig.AppInsightsConnectionString;
});

// Configure Microsoft Orleans Client
if (builder.Environment.IsDevelopment())
{
    builder.Host.UseOrleansClient((_, clientBuilder) =>
    {
        clientBuilder.Configure<ClusterOptions>(_ => { })
            .UseLocalhostClustering();
    });
}
else
{
    builder.Host.UseOrleansClient((_, clientBuilder) =>
    {
        clientBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "ShoppingApp";
                options.ServiceId = "ShoppingAppService";
            })
            .UseAzureStorageClustering(options =>
            {
                options.ConfigureTableServiceClient(GlobalConfig.AzureStorageConnection);
            });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this
    // for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();