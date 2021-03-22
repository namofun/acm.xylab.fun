using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteSite.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.XylabModule.Services
{
    public class HybridEmailSender : IEmailSender
    {
        private readonly Action<ILogger, string, Exception> _emailSending;
        private readonly ILogger<HybridEmailSender> _logger;
        private readonly ITelemetryClient _telemetry;
        private readonly HybridEmailOptions _apiKey;

        public HybridEmailSender(
            ILogger<HybridEmailSender> logger,
            IOptions<HybridEmailOptions> options,
            ITelemetryClient telemetry)
        {
            _logger = logger;
            _apiKey = options.Value;
            _telemetry = telemetry;

            _emailSending = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(17362),
                formatString: "An email will be sent to {email}");
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            if (email?.EndsWith("@qq.com") ?? false)
            {
                return SendQQAsync(email, subject, message);
            }
            else
            {
                return SendGridAsync(email, subject, message);
            }
        }

        private Task SendGridAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(_apiKey.SendGridKey);

            var msg = new SendGridMessage
            {
                From = new EmailAddress("acm@xylab.fun", "小羊实验室"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message,
            };

            msg.AddTo(new EmailAddress(email));
            msg.SetClickTracking(false, false);

            _emailSending(_logger, email, null);
            var telemetry = _telemetry;
            var user = _apiKey.SendGridUser;

            Task.Run(async () =>
            {
                var startTime = DateTimeOffset.Now;
                Exception exception = null;

                try
                {
                    var response = await client.SendEmailAsync(msg);
                    if (!response.IsSuccessStatusCode)
                    {
                        exception = new Exception("Response does not indicate a success status code.");
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                telemetry.TrackDependency(
                    dependencyTypeName: "SendGrid",
                    dependencyName: "SendGrid",
                    target: user,
                    data: email,
                    startTime: startTime,
                    duration: DateTimeOffset.Now - startTime,
                    resultCode: exception == null ? "OK" : exception.Message,
                    success: exception == null);
            });

            return Task.CompletedTask;
        }

        private Task SendQQAsync(string email, string subject, string message)
        {
            var msg = new MailMessage();
            msg.To.Add(email);
            msg.From = new MailAddress("webmaster@90yang.com", "小羊实验室");

            msg.Subject = subject;
            msg.SubjectEncoding = Encoding.UTF8;

            msg.IsBodyHtml = true;
            msg.Body = message;
            msg.BodyEncoding = Encoding.UTF8;

            var client = new SmtpClient
            {
                Host = "smtp.qq.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(_apiKey.QQUser, _apiKey.QQKey)
            };

            _emailSending(_logger, email, null);
            var telemetry = _telemetry;

            Task.Run(async () =>
            {
                var startTime = DateTimeOffset.Now;
                Exception exception = null;

                try
                {
                    await client.SendMailAsync(msg);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                telemetry.TrackDependency(
                    dependencyTypeName: "Smtp",
                    dependencyName: $"{client.Host}:{client.Port}",
                    target: msg.From.Address,
                    data: string.Join(';', msg.To.Select(a => a.Address)),
                    startTime: startTime,
                    duration: DateTimeOffset.Now - startTime,
                    resultCode: exception == null ? "OK" : exception.Message,
                    success: exception == null);
            });

            return Task.CompletedTask;
        }
    }
}
