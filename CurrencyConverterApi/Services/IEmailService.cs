using System.Threading.Tasks;

namespace CurrencyConverterApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}
