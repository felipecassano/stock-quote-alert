using StockQuoteAlert.Models;
using StockQuoteAlert.Services;

namespace stock_quote_alert.Tests;

public class ValidateServiceTest
{
    private readonly ArgsParser _ticker = new ArgsParser();

    [Fact]
    public void TestandoArgumentosValidos()
    {
        // Cenário
        string[] args = ["PETR4", "22.67", "22.59"];

        // Acão
        Args? result = _ticker.ValidateArgs(args);

        //  Validação
        Assert.NotNull(result);
        Assert.Equal("PETR4", result.Ticker);
        Assert.Equal(22.67m, result.SellPrice);
        Assert.Equal(22.59m, result.BuyPrice);

        //  Padrão AAA (Arrange, Act, Assert)
    }
}
