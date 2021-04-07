using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Refactored.Email;

namespace Examples.NetFramework.Mvc.Controllers {

	public class EmailController : ApiController
    {
        // GET api/Email/Simple
		[HttpGet]
		[Route("api/email/simple")]
		public string Simple()
        {
			
			Email email = new Email();
			// Set the Mail Merge Field Pattern:
			email.FieldPattern = "[{0}]";

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
			email.SendEmail(null, "info@refoster.com.au", subject, htmlContent, textContent);
			// return new string[] { "value1", "value2" };

			return "Email Sent";
		}



		// GET api/Email/Template
		[HttpGet]
		[Route("api/email/template")]
		public string Template() {
			Email email = new Email();
			// Set the Mail Merge Field Pattern:
			email.FieldPattern = "[{0}]";

			// Set the Base URL for hyperlinks found in the message templates
			email.WebBaseUrl = "http://refactored.com.au";

			// Set the directory containing the message templates
			email.MailTemplateDirectory = System.Web.Hosting.HostingEnvironment.MapPath("~/templates/");

			// Create a new parameters collection to hold our mail-merge fields:
			NameValueCollection parameters = new NameValueCollection();
			parameters.Add("date", DateTime.Today.ToString());
			string subject;

			// We now want to parse the message templates, inserting the mail merge data and extracting the subject:
			string htmlContent = email.ParseMessageTemplate("htmlTemplate.html", parameters, out subject);
			string textContent = email.ParseMessageTemplate("textTemplate.txt", parameters);

			// Send the email with both html and plain text content.
			email.SendEmail("no-reply@refoster.com.au", "info@refoster.com.au", subject, htmlContent, textContent);

			return "Templated Email Sent";
		}


	}
}
