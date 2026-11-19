namespace StockQuoteAlert.Models;

public record Args(string Ticker, decimal SellPrice, decimal BuyPrice);
