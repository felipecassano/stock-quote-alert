// See https://aka.ms/new-console-template for more information
using System.Globalization;
using StockQuoteAlert;
using StockQuoteAlert.Services;
using StockQuoteAlert.Models;


Ticker t = new Ticker();
var argsResponse = t.Validate(args);

if (argsResponse is null)
{
    return;
}

Console.WriteLine("testando");
QuoteService s = new QuoteService();

var response = await s.GetQuoteAsync(argsResponse.Ticker);

if (response is null)
{

}
Console.WriteLine(response);



namespace StockQuoteAlert
{
    public class Ticker
    {

        public Args? Validate(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Argumento passado deve ser igual a 3");
                return null;
            }
            decimal sellPrice;
            decimal buyPrice;

            bool successSellPrice = decimal.TryParse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture, out sellPrice);
            bool successBuyPrice = decimal.TryParse(args[2], NumberStyles.Number, CultureInfo.InvariantCulture, out buyPrice);
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
                Console.WriteLine("Regras para os valores:");
                Console.WriteLine("Valor de venda não pode ser 0");
                Console.WriteLine("Valor de compra não pode ser 0");
                Console.WriteLine("Valor de venda deve ser maior que o valor de compra");
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
}
