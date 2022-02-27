using Refactored.Email;
using Refactored.Email.Configuration;
using System;
using System.Collections.Specialized;

namespace Examples.NetFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            //SimpleSend();

            UsingTemplateFiles();

        }


        public static void SimpleSend()
        {

            Sender email = new Sender(new EmailOptions
            {
                // Set the Mail Merge Field Pattern:
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
            string htmlContent = email.ParseMessageTemplateContent(html, parameters, out subject);
            string textContent = email.ParseMessageTemplateContent(text, parameters);


            // Send the email with both html and plain text content.
            email.Send("no-reply@youritteam.com.au", "info@youritteam.com.au", subject, htmlContent, textContent);
        }

        public static void UsingTemplateFiles()
        {
            Sender email = new Sender(new EmailOptions
            {
                // Set the Mail Merge Field Pattern:
                FieldPattern = "[{0}]",

                // Set the Base URL for hyperlinks found in the message templates
                WebBaseUrl = "http://refactored.com.au",

                // Set the directory containing the message templates
                MailTemplateDirectory = $"{AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))}templates\\"
            });
            // Create a new parameters collection to hold our mail-merge fields:
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("date", DateTime.Today.ToString());
            string subject;

            // We now want to parse the message templates, inserting the mail merge data and extracting the subject:
            string htmlContent = email.ParseMessageTemplate("htmlTemplate.html", parameters, out subject);
            string textContent = email.ParseMessageTemplate("textTemplate.txt", parameters);

            // Send the email with both html and plain text content.
            email.Send("no-reply@youritteam.com.au", "info@youritteam.com.au", subject, htmlContent, textContent);
        }
    }
}
