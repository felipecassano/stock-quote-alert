using System.Globalization;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Services;

public class ArgsParser
{

    public Args? ValidateArgs(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Argumento passado deve ser igual a 3");
            Console.WriteLine("Uso: stock-quote-alert <ATIVO> < PRECO_VENDA > < PRECO_COMPRA >");
            return null;
        }
        string sellPrice = args[1];
        string buyPrice = args[2];
        // Aceita virgula como argumento
        if (sellPrice.Contains(","))
        {
            sellPrice = sellPrice.Replace(",", ".");
        }
        if (buyPrice.Contains(","))
        {
            buyPrice = buyPrice.Replace(",", ".");
        }

        //  Trocado de NumberStyles.Number para NumberStyles.AllowDecimalPoint por que 22,34 virava 2234 
        bool successSellPrice = decimal.TryParse(sellPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal parsedSellPrice);
        bool successBuyPrice = decimal.TryParse(buyPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal parsedBuyPrice);
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

        if (parsedSellPrice <= 0 || parsedBuyPrice <= 0 || parsedBuyPrice == parsedSellPrice || parsedSellPrice < parsedBuyPrice)
        {
            Console.WriteLine("Regras para as entradas:");
            Console.Write("Valor de venda não pode ser 0 ");
            Console.WriteLine(parsedSellPrice <= 0 ? "X" : "✓");
            Console.Write("Valor de compra não pode ser 0 ");
            Console.WriteLine(parsedBuyPrice <= 0 ? "X" : "✓");
            Console.Write("Valor de venda deve ser maior que o valor de compra ");
            Console.WriteLine(parsedSellPrice <= parsedBuyPrice ? "X" : "✓");

            return null;


        }

        string ticker = args[0].Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(ticker))
        {
            Console.WriteLine("O nome da ação não pode ser nulo");
            return null;

        }
        Console.WriteLine($"Usuário quer analisar a ação {ticker} com preço de venda: {parsedSellPrice} e preço de compra: {parsedBuyPrice} ");
        Console.WriteLine("Pressione Ctrl+C para encerrar.");
        return new Args(Ticker: ticker, SellPrice: parsedSellPrice, BuyPrice: parsedBuyPrice);




    }

}