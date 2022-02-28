using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Mailing;
using SatelliteSite.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SatelliteSite.XylabModule.Services
{
    public class LogicAppsEmailSender : IEmailSender
    {
        private readonly IConfigurationRegistry _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LogicAppsEmailSender> _logger;

        public LogicAppsEmailSender(
            IConfigurationRegistry configuration,
            HttpClient httpClient,
            ILogger<LogicAppsEmailSender> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            string requestUrl = await _configuration.GetStringAsync("email_sender_url");

            _logger.LogInformation("An email will be sent to {email}", email);

            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    requestUrl,
                    new LogicAppsEmailRequest()
                    {
                        To = email,
                        Content = message,
                        IsHtml = true,
                        Subject = subject,
                    });

            response.EnsureSuccessStatusCode();
        }
    }
}
