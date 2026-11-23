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

    [Fact]
    public async Task CheckAsync_PrecoAbaixoDaCompra_EnviaEmailDeCompra()
    {
        // Cenário
        var quotes = new FakeQuoteService { Price = 22.00m };
        var email = new FakeEmailSender();
        var service = CreateService(quotes, email);

        // Ação
        int newState = await service.CheckAsync(0);

        //  Validação
        Assert.Equal(-1, newState);
        Assert.Single(email.Subjects);
        Assert.Contains("COMPRA", email.Subjects[0]);
    }

    [Fact]
    public async Task CheckAsync_PrecoNaFaixaNeutra_NaoEnviaEmail()
    {
        // Cenário
        var quotes = new FakeQuoteService { Price = 22.63m };
        var email = new FakeEmailSender();
        var service = CreateService(quotes, email);

        // Ação
        int newState = await service.CheckAsync(0);

        //  Validação
        Assert.Equal(0, newState);
        Assert.Empty(email.Subjects);
    }

    [Fact]
    public async Task CheckAsync_PrecoContinuaAcimaDaVenda_NaoEnviaNovoEmail()
    {
        // Cenário
        var quotes = new FakeQuoteService { Price = 23.00m };
        var email = new FakeEmailSender();
        var service = CreateService(quotes, email);

        // Ação
        int newState = await service.CheckAsync(1);

        //  Validação: continua na mesma faixa, então não repete o e-mail
        Assert.Equal(1, newState);
        Assert.Empty(email.Subjects);
    }

    [Fact]
    public async Task CheckAsync_FalhaNaApiDeCotacao_MantemEstadoAtual()
    {
        // Cenário: a API não retornou cotação
        var quotes = new FakeQuoteService { Price = null };
        var email = new FakeEmailSender();
        var service = CreateService(quotes, email);

        // Ação
        int newState = await service.CheckAsync(1);

        //  Validação
        Assert.Equal(1, newState);
        Assert.Empty(email.Subjects);
    }

    [Fact]
    public async Task CheckAsync_FalhaNoEnvioDoEmail_LancaExcecao()
    {
        // Cenário
        var quotes = new FakeQuoteService { Price = 23.00m };
        var email = new FakeEmailSender { ShouldFail = true };
        var service = CreateService(quotes, email);

        // Ação + Validação
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckAsync(0));
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