using StockQuoteAlert.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockQuoteAlert.Config;
using StockQuoteAlert.Interfaces;
using System.Net.Http.Headers;

var argsResponse = new ArgsParser().ValidateArgs(args);
if (argsResponse is null) return 1;


var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory,
});
builder.Configuration.AddUserSecrets<Program>();

var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (string.IsNullOrWhiteSpace(emailSettings?.ToEmail) || string.IsNullOrWhiteSpace(emailSettings?.Password))
{
    Console.WriteLine("Configure EmailSettings:ToEmail e EmailSettings:Password (veja o README).");
    return 1;
}

builder.Services.AddSingleton(argsResponse);
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddHttpClient<QuoteService>(client =>
{
    client.BaseAddress = new Uri("https://brapi.dev/api/");

    string? brapiKey = builder.Configuration["BRAPI_KEY"];

    if (!string.IsNullOrWhiteSpace(brapiKey))
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", brapiKey);
})
.AddStandardResilienceHandler();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<StockAlertService>();
builder.Services.AddHostedService<PeriodicHostedService>();

using var host = builder.Build();
await host.RunAsync();
return 0;
