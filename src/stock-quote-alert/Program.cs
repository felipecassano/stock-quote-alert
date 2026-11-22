// See https://aka.ms/new-console-template for more information
using System.Globalization;
using StockQuoteAlert;
using StockQuoteAlert.Services;
using StockQuoteAlert.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockQuoteAlert.Config;
using StockQuoteAlert.Interface;

var argsResponse = new Ticker().Validate(args);
if (argsResponse is null) return;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddSingleton(argsResponse);
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddHttpClient<QuoteService>(client =>
{
    client.BaseAddress = new Uri("https://brapi.dev/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddHostedService<PeriodicHostedService>();

using var host = builder.Build();
await host.RunAsync();