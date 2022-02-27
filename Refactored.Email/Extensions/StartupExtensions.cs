#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refactored.Email.Configuration;
using System.IO;
using System.Net;

namespace Refactored.Email.Extensions
{
    public static class StartupExtensions
    {
        /// <summary>
        /// Adds the Email Sender service as a Singleton
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
        {
            services.AddEmailSender(config.SetupEmailOptions(env));

            return services;
        }

        /// <summary>
        /// Retrieves and sets up the EmailOptions with additional settings.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static EmailOptions SetupEmailOptions(this IConfiguration config, IHostEnvironment env)
        {
            var emailSection = config.GetSection("Email");
            SmtpSection smtp = null;
            if (emailSection != null)
            {
                var smtpSection = emailSection.GetSection("SMTP");
                if (smtpSection != null)
                {
                    int port = 25;
                    if (!string.IsNullOrEmpty(smtpSection.GetSection("Port")?.Value))
                    {
                        port = int.Parse(smtpSection.GetSection("Port").Value);
                    }
                    smtp = new SmtpSection()
                    {
                        Host = smtpSection.GetSection("Host").Value,
                        Port = port,
                        From = smtpSection.GetSection("From").Value,
                        Credentials = new NetworkCredential(
                            smtpSection.GetSection("Username").Value,
                            smtpSection.GetSection("Password").Value
                        )
                    };
                }
            }

            var options = new EmailOptions();
            config.GetSection(nameof(EmailOptions)).Bind(options);
            if (string.IsNullOrWhiteSpace(options.ContentRootPath))
            {
                var wwwRoot = Path.Combine(env.ContentRootPath, "wwwroot");
                if (Directory.Exists(wwwRoot))
                {
                    options.ContentRootPath = wwwRoot;
                }
                else
                {
                    options.ContentRootPath = env.ContentRootPath;
                }
            }
            options.SmtpSettings = smtp;

            return options;
        }

        public static IServiceCollection AddEmailSender(this IServiceCollection services, EmailOptions options)
        {
            services.AddSingleton(sp => new Sender(options));

            return services;
        }

    }
}
#endif