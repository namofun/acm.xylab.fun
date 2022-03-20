using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Mailing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SatelliteSite.XylabModule.Services
{
    public class LogicAppsEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LogicAppsEmailSender> _logger;
        private readonly string _url;

        public LogicAppsEmailSender(
            HttpClient httpClient,
            ILogger<LogicAppsEmailSender> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _url = configuration.GetConnectionString("LogicAppsMailSender");
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("An email will be sent to {email}", email);

            JsonContent content = JsonContent.Create(new LogicAppsEmailRequest()
            {
                To = email,
                Content = message,
                IsHtml = true,
                Subject = subject,
            });

            // Cannot use PostAsJsonAsync as the following
            // https://stackoverflow.com/questions/47816551/postasjsonasync-doesnt-seem-to-post-body-parameters
            await content.LoadIntoBufferAsync();
            HttpResponseMessage response = await _httpClient.PostAsync(_url, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
