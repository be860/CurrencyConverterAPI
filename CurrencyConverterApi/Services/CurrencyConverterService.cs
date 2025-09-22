using CurrencyConverterApi.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyConverterApi.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _symbol;
        private readonly string _apiKey;

        public CurrencyConverterService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["ExchangeRates:BaseUrl"]!;
            _symbol = config["ExchangeRates:Symbol"] ?? "SLE";
            _apiKey = config["ExchangeRates:ApiKey"]!;
        }

        public async Task<ConversionResponse> ConvertUsdToSllAsync(decimal amountUsd)
        {
            // 1) Build the request
            var url = $"{_baseUrl}/latest?base=USD&symbols={_symbol}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            // 2) Add the API key header
            req.Headers.Add("apikey", _apiKey);

            // 3) Send and ensure success
            var resp = await _httpClient.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            // 4) Parse JSON
            var text = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            var rates = doc.RootElement.GetProperty("rates");

            // 5) Pull the rate
            if (!rates.TryGetProperty(_symbol, out var rateElem))
                throw new InvalidOperationException($"No rate for '{_symbol}' returned.");

            var rate = rateElem.GetDecimal();
            var converted = amountUsd * rate;

            // 6) Return the response
            return new ConversionResponse
            {
                AmountUsd = amountUsd,
                ExchangeRate = rate,
                AmountSll = Math.Round(converted, 2),
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
