namespace CurrencyConverterApi.Models
{
    public class ConversionResponse
    {
        /// <summary>
        /// Original amount in USD.
        /// </summary>
        public decimal AmountUsd { get; set; }

        /// <summary>
        /// Converted amount in SLL.
        /// </summary>
        public decimal AmountSll { get; set; }

        /// <summary>
        /// Exchange rate used (SLL per 1 USD).
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Timestamp when conversion happened.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
