using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StockQuoteAlert.Config;
using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;
using StockQuoteAlert.Services;

namespace stock_quote_alert.Tests;

public class StockAlertServiceTest
{
    [Fact]
    public async Task CheckAsync_PrecoAcimaDaVenda_EnviaEmailDeVenda()
    {
        // Cenário
        var quotes = new FakeQuoteService { Price = 23.00m };
        var email = new FakeEmailSender();
        var service = CreateService(quotes, email);

        // Ação
        int newState = await service.CheckAsync(0);

        //  Validação
        Assert.Equal(1, newState);
        Assert.Single(email.Subjects);
        Assert.Contains("VENDA", email.Subjects[0]);
    }

    private static StockAlertService CreateService(IQuoteService quotes, IEmailSender email) =>
        new(new Args("PETR4", 22.67m, 22.59m), quotes, email,
            NullLogger<StockAlertService>.Instance,
            Options.Create(new EmailSettings { ToEmail = "teste@exemplo.com" }));

    private class FakeQuoteService : IQuoteService
    {
        public decimal? Price { get; set; }

        public Task<Quote?> GetQuoteAsync(string ticker) =>
            Task.FromResult<Quote?>(Price is null ? null : new Quote(ticker, "", Price.Value, 0, "BRL"));
    }

    private class FakeEmailSender : IEmailSender
    {
        public List<string> Subjects { get; } = [];
        public bool ShouldFail { get; set; }

        public Task SendEmailAsync(string email, string subject, string message, bool isHtml)
        {
            if (ShouldFail) throw new InvalidOperationException("SMTP indisponível");
            Subjects.Add(subject);
            return Task.CompletedTask;
        }
    }
}