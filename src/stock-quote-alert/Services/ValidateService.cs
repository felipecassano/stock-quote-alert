using System.Globalization;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Services;

public class Ticker
{

    public Args? ValidateArgs(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Argumento passado deve ser igual a 3");
            return null;
        }
        decimal sellPrice;
        decimal buyPrice;

        //  Trocado de NumberStyles.Number para NumberStyles.AllowDecimalPoint por que 22,34 virava 2234 

        bool successSellPrice = decimal.TryParse(args[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out sellPrice);
        bool successBuyPrice = decimal.TryParse(args[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out buyPrice);
        if (!successSellPrice)
        {
            Console.WriteLine(" Erro ao tentar converter o preço de venda");
            return null;

        }
        if (!successBuyPrice)
        {
            Console.WriteLine(" Erro ao tentar converter o preço de compra");
            return null;

        }

        if (sellPrice <= 0 || buyPrice <= 0 || buyPrice == sellPrice || sellPrice < buyPrice)
        {
            Console.WriteLine("Regras para as entradas:");
            Console.Write("Valor de venda não pode ser 0 ");
            Console.WriteLine(sellPrice <= 0 ? "X" : "✓");
            Console.Write("Valor de compra não pode ser 0 ");
            Console.WriteLine(buyPrice <= 0 ? "X" : "✓");
            Console.Write("Valor de venda deve ser maior que o valor de compra ");
            Console.WriteLine(sellPrice <= buyPrice ? "X" : "✓");

            return null;


        }

        string ticker = args[0].Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(ticker))
        {
            Console.WriteLine("O nome da ação não pode ser nulo");
            return null;

        }
        Console.WriteLine($"Usuário quer analisar a ação {ticker} com preço de venda: {sellPrice} e preço de compra: {buyPrice} ");
        return new Args(Ticker: ticker, SellPrice: sellPrice, BuyPrice: buyPrice);




    }

}