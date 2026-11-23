using StockQuoteAlert.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockQuoteAlert.Config;
using StockQuoteAlert.Interface;
using System.Net.Http.Headers;

var argsResponse = new Ticker().ValidateArgs(args);
if (argsResponse is null) return;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddSingleton(argsResponse);
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddHttpClient<QuoteService>(client =>
{
    client.BaseAddress = new Uri("https://brapi.dev/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["BRAPI_KEY"]);
});
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<StockAlertService>();
builder.Services.AddHostedService<PeriodicHostedService>();

using var host = builder.Build();
await host.RunAsync();
