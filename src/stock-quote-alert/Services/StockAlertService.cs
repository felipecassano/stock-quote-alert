using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockQuoteAlert.Config;
using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Services;

public class StockAlertService
{
    private readonly Args _args;
    private readonly IQuoteService _quoteService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<StockAlertService> _logger;
    private readonly EmailSettings _emailSettings;


    public StockAlertService(Args args, IQuoteService quoteService,
                               IEmailSender emailSender, ILogger<StockAlertService> logger, IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
        _args = args;
        _quoteService = quoteService;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<int> CheckAsync(int currentState)
    {
        var Quote = await _quoteService.GetQuoteAsync(_args.Ticker);
        if (Quote is null) return currentState;

        decimal cotacao = Quote.RegularMarketPrice;
        _logger.LogInformation($"Cotação: {Quote.RegularMarketPrice}");

        int newState = cotacao > _args.SellPrice ? 1 : cotacao < _args.BuyPrice ? -1 : 0;

        if (newState != currentState)
        {
            if (newState == 1)
            {
                var subject = $"{_args.Ticker}: VENDA - cotação R$ {cotacao:N2} acima de R$ {_args.SellPrice:N2}";

                var body = $"<p>A ação <b>{_args.Ticker}</b> está cotada a <b>R$ {cotacao:N2}</b>, " +
               $"acima do preço de venda configurado (R$ {_args.SellPrice:N2}).</p>";

                await _emailSender.SendEmailAsync(_emailSettings.ToEmail, subject, body, isHtml: true);
                _logger.LogInformation("Email enviado para venda");
            }
            if (newState == -1)
            {
                var subject = $"{_args.Ticker}: COMPRA - cotação R$ {cotacao:N2} abaixo de R$ {_args.BuyPrice:N2}";

                var body = $"<p>A ação <b>{_args.Ticker}</b> está cotada a <b>R$ {cotacao:N2}</b>, " +
               $"abaixo do preço de compra configurado (R$ {_args.BuyPrice:N2}).</p>";

                await _emailSender.SendEmailAsync(_emailSettings.ToEmail, subject, body, isHtml: true);
                _logger.LogInformation("Email enviado para compra");

            }

        }
        else
        {

            _logger.LogInformation("Nenhum email foi enviado, ação se manteve no mesmo estado da anterior");
        }

        return newState;

    }
}