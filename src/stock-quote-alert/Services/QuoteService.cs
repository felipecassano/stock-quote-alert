using System.Text.Json;
using Microsoft.Extensions.Logging;
namespace StockQuoteAlert.Services;


public record Quote(
    string Symbol,
    string ShortName,
    decimal RegularMarketPrice,
    decimal RegularMarketChangePercent,
    string Currency);

public record QuoteResponse(Quote[]? Results);

public class QuoteService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public QuoteService(HttpClient httpClient, ILogger<QuoteService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Quote?> GetQuoteAsync(string Ticker)
    {
        try
        {
            var url = $"quote/{Ticker}";
            _logger.LogInformation(url);


            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Erro: {response.StatusCode}");
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
