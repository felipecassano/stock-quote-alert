using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
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
    private const string BaseUrl = "https://brapi.dev/api";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private HttpClient _httpClient;
    public QuoteService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }
    public async Task<Quote?> GetQuoteAsync(string Ticker)
    {
        var config = new ConfigurationBuilder()
          .AddUserSecrets<Program>()
          .Build();
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config["BRAPI_KEY"]);
            var url = $"{BaseUrl}/quote/{Ticker}";
            Console.WriteLine(url);

        
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro: {response.StatusCode}");
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();


            QuoteResponse data = JsonSerializer.Deserialize<QuoteResponse>(responseBody, JsonOptions);

            return data?.Results?[0];


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Problema encontrado: {ex}");
            return null;

        }


    }
}
