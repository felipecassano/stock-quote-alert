using Microsoft.Extensions.Logging;
using StockQuoteAlert.Interface;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Services;

public class StockAlertService
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

    public async Task<int> CheckAsync(int currentState)
    {
        _logger.LogInformation("Sample Service did something");
        var Quote = await _quoteService.GetQuoteAsync(_args.Ticker);
        if (Quote is null) return currentState;

        decimal cotacao = Quote.RegularMarketPrice;
        Console.WriteLine($"Cotação: {Quote.RegularMarketPrice}");

        int newState = cotacao > _args.SellPrice ? 1 : cotacao < _args.BuyPrice ? -1 : 0;

        if (newState != currentState)
        {
            if (newState == 1)
            {
                _logger.LogInformation("Email enviado para venda");
            }
            if (newState == -1)
            {
                _logger.LogInformation("Email enviado para compra");

            }

        }
        else
        {

            _logger.LogInformation("Nenhum email foi enviado, ação se manteve no mesmo estado do anterior");
        }

        return newState;

    }
}