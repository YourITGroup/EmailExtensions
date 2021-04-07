using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Refactored.Email;

namespace Examples.NetFramework
{
    class Program
    {
        static void Main(string[] args)
        {
			//SimpleSend();

			UsingTemplateFiles();

		}


		public static void SimpleSend() {
			// Set the Mail Merge Field Pattern:
			Email.FieldPattern = "[{0}]";

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
			string htmlContent = Email.ParseMessageTemplateContent(html, parameters, out subject);
			string textContent = Email.ParseMessageTemplateContent(text, parameters);

			// Send the email with both html and plain text content.
			Email.SendEmail("no-reply@refoster.com.au", "info@refoster.com.au", subject, htmlContent, textContent);
		}
    
		public static void UsingTemplateFiles() {
			// Set the Mail Merge Field Pattern:
			Email.FieldPattern = "[{0}]";

			// Set the Base URL for hyperlinks found in the message templates
			Email.WebBaseUrl = "http://refactored.com.au";

			// Set the directory containing the message templates
			Email.MailTemplateDirectory = $"{AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))}templates\\";

			// Create a new parameters collection to hold our mail-merge fields:
			NameValueCollection parameters = new NameValueCollection();
			parameters.Add("date", DateTime.Today.ToString());
			string subject;

			// We now want to parse the message templates, inserting the mail merge data and extracting the subject:
			string htmlContent = Email.ParseMessageTemplate("htmlTemplate.html", parameters, out subject);
			string textContent = Email.ParseMessageTemplate("textTemplate.txt", parameters);

			// Send the email with both html and plain text content.
			Email.SendEmail("no-reply@refoster.com.au", "info@refoster.com.au", subject, htmlContent, textContent);
		}
	}
}
