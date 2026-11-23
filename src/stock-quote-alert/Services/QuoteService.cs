using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Services;

public record Quote(
    string Symbol,
    string ShortName,
    decimal RegularMarketPrice,
    decimal RegularMarketChangePercent,
    string Currency);

public record QuoteResponse(Quote[]? Results);

public class QuoteService : IQuoteService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public QuoteService(HttpClient httpClient, ILogger<QuoteService> logger, IHostApplicationLifetime lifetime)
    {
        _httpClient = httpClient;
        _logger = logger;
        _lifetime = lifetime;
    }

    public async Task<Quote?> GetQuoteAsync(string Ticker)
    {
        try
        {
            var url = $"quote/{Ticker}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode.ToString() == "NotFound")
                {
                    _logger.LogInformation($"Ativo {Ticker} não encontrado. Encerrando a aplicação");
                    Environment.ExitCode = 1;
                    _lifetime.StopApplication();
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogInformation($"Precisa de token para consultar o {Ticker}. Configure o BRAPI_KEY");
                    Environment.ExitCode = 1;
                    _lifetime.StopApplication();
                }
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            QuoteResponse? data = JsonSerializer.Deserialize<QuoteResponse>(responseBody, JsonOptions);

            return data?.Results?[0];

        }
        catch (Exception ex)
        {
            _logger.LogError($"Problema encontrado: {ex}");
            return null;

        }


    }
}
