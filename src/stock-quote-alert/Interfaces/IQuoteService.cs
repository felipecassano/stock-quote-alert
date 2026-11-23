using StockQuoteAlert.Services;

namespace StockQuoteAlert.Interfaces;

public interface IQuoteService
{
    Task<Quote?> GetQuoteAsync(string ticker);
}