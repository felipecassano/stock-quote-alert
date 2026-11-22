using Microsoft.Extensions.Logging;
using StockQuoteAlert.Interface;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Services;

class StockAlertService
{
    private readonly Args _args;
    private readonly QuoteService _quoteService;
    private IEmailSender _emailSender;
    private readonly ILogger<StockAlertService> _logger;

    public StockAlertService(Args args, QuoteService quoteService,
                               IEmailSender emailSender, ILogger<StockAlertService> logger)
    {
        _args = args;
        _quoteService = quoteService;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task CheckAsync()
    {
        await Task.Delay(100);
        _logger.LogInformation("Sample Service did something");
        var Quote = await _quoteService.GetQuoteAsync(_args.Ticker);
        if (Quote is null) return;
        decimal cotacaoAtual = Quote.RegularMarketPrice;
        Console.WriteLine($"Cotação: {Quote.RegularMarketPrice}");

        if (cotacaoAtual > _args.SellPrice)
        {
            Console.WriteLine("Enviando email para venda");
        }
        if (cotacaoAtual < _args.BuyPrice)
        {
            Console.WriteLine("Enviando email para compra");
        }
    }
}