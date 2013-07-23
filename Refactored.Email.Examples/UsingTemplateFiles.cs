using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Refactored.Email.Examples
{
    public class UsingTemplateFiles
    {
        public static void SendEmail()
        {
            // Set the Mail Merge Field Pattern:
            Email.FieldPattern = "[{0}]";
            
            // Set the Base URL for hyperlinks found in the message templates
            Email.WebBaseUrl = "http://refactored.com.au";
            
            // Set the directory containing the message templates
            Email.MailTemplateDirectory = @"C:\Mail Templates";

            // Create a new parameters collection to hold our mail-merge fields:
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("date", DateTime.Today.ToString());
            string subject;

            // We now want to parse the message templates, inserting the mail merge data and extracting the subject:
            string htmlContent = Email.ParseMessageTemplate("htmlTemplate.htm", parameters, out subject);
            string textContent = Email.ParseMessageTemplate("textTemplate.txt", parameters);

            // Send the email with both html and plain text content.
            Email.SendEmail("no-reply@refoster.com.au", "info@refoster.com.au", subject, htmlContent, textContent);

        }
    }
}
