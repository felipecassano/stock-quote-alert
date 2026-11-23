using System.Text.Json;
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

    public QuoteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Quote?> GetQuoteAsync(string Ticker)
    {
        try
        {
            // relativo ao BaseAddress "https://brapi.dev/api/" (sem barra no início!)
            var url = $"quote/{Ticker}";
            Console.WriteLine(url);


            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro: {response.StatusCode}");
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();


            QuoteResponse data = JsonSerializer.Deserialize<QuoteResponse>(responseBody, JsonOptions);
            Console.WriteLine(data?.Results?[0].RegularMarketPrice);
            return data?.Results?[0];


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Problema encontrado: {ex}");
            return null;

        }


    }
}
