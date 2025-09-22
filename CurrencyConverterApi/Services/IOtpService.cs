using System.Threading.Tasks;

namespace CurrencyConverterApi.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(string email);
        Task<bool> ValidateOtpAsync(string email, string code);
    }
}
