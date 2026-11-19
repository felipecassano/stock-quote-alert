

namespace Quote
{
    class QuoteService
    {
        public async Task<string> GetQuote()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", "hs12RXzmiVGAipTo42zR9r");
                    var response = await client.GetAsync("https://brapi.dev/api/quote/PETR4");
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Problema encontrado: {ex}");
                    return "";
                }

            }
        }
    }

}

