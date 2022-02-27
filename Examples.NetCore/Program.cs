using Microsoft.Extensions.Configuration;
using Refactored.Email;
using System;
using System.Collections.Specialized;
using System.IO;

namespace Examples.NetCore
{
    class Program
    {
        static IConfiguration _configuration;

        static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        //.AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .Build();

            //SimpleSend();

            TemplateSend();

        }



        public static void SimpleSend()
        {
            Sender sender = new Sender(new Refactored.Email.Configuration.EmailOptions
            {
                FieldPattern = "[{0}]"
            });

            // Set up our HTML and Plain Text templates:
            string html = @"<html>
    <head>
        <title>Simple Html Email</title>
    </head>
    <body>
        <p>Hello World!</p>
        <p>This email contains a link to the <a href=""http://refactored.com.au"">Refactored Website</a></p>
        <p>It also contains a mail-merge field delimited by [ and ]: [date]</p>
    </body>
</html>";

            string text = @"Hello World!
This email contains a link to the Refactored Website: http://refactored.com.au
IT also contains a mail-merge field delimited by [ and ]: [date]";

            // Create a new parameters collection to hold our mail-merge fields:
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("date", DateTime.Today.ToString());
            string subject;

            // We now want to parse the message templates, inserting the mail merge data and extracting the subject:
            string htmlContent = sender.ParseMessageTemplateContent(html, parameters, out subject);
            string textContent = sender.ParseMessageTemplateContent(text, parameters);



            // Send the email with both html and plain text content.
            sender.Send("no-reply@youritteam.com.au", "info@youritteam.com.au", subject, htmlContent, textContent);
        }



        public static void TemplateSend()
        {
            Sender sender = new Sender(new Refactored.Email.Configuration.EmailOptions
            {
                // Set the Mail Merge Field Pattern:
                FieldPattern = "[{0}]",

                // Set the Base URL for hyperlinks found in the message templates
                WebBaseUrl = "http://refactored.com.au",

                // Set the directory containing the message templates
                MailTemplateDirectory = Directory.GetCurrentDirectory() + "/templates"
            });

            // Create a new parameters collection to hold our mail-merge fields:
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("date", DateTime.Today.ToString());
            string subject;

            // We now want to parse the message templates, inserting the mail merge data and extracting the subject:
            string htmlContent = sender.ParseMessageTemplate("htmlTemplate.html", parameters, out subject);
            string textContent = sender.ParseMessageTemplate("textTemplate.txt", parameters);

            // Send the email with both html and plain text content.
            sender.Send("no-reply@youritteam.com.au", "info@youritteam.com.au", subject, htmlContent, textContent);


        }
    }
}
