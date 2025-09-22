using CurrencyConverterApi.Models;
using System.Threading.Tasks;

namespace CurrencyConverterApi.Services
{
    public interface ICurrencyConverterService
    {
        /// <summary>
        /// Converts USD to SLL.
        /// </summary>
        Task<ConversionResponse> ConvertUsdToSllAsync(decimal amountUsd);
    }
}
