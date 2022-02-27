using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Refactored.Email;
using System;
using System.Collections.Specialized;

namespace Examples.NetCore.Mvc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {


        private readonly ILogger<EmailController> _logger;
        private readonly Sender _email;

        public EmailController(ILogger<EmailController> logger, Sender email)
        {
            _logger = logger;
            _email = email;
        }

        // GET /Email/Simple
        [HttpGet]
        [Route("simple")]
        public string Simple()
        {

            // Set up our HTML and Plain Text templates:
            string html = @"<html>
    <head>
        <title>Simple Html Email</title>
    </head>
    <body>
        <p>Hello World!</p>
        <p>This email contains a link to the <a href=""/test"">Refactored Website</a></p>
        <p>It also contains a mail-merge field delimited by [ and ]: [date]</p>
    </body>
</html>";

            string text = @"Hello World!
This email contains a link to the Refactored Website: http://refactored.com.au
IT also contains a mail-merge field delimited by [ and ]: [date]";

            // Create a new parameters collection to hold our mail-merge fields:
            NameValueCollection parameters = new NameValueCollection
            {
                { "date", DateTime.Today.ToString() }
            };
            string subject;

            // We now want to parse the message templates, inserting the mail merge data and extracting the subject:
            string htmlContent = _email.ParseMessageTemplateContent(html, parameters, out subject);
            string textContent = _email.ParseMessageTemplateContent(text, parameters);


            // Send the email with both html and plain text content.
            _email.Send(null, "info@youritteam.com.au", subject, htmlContent, textContent);
            // return new string[] { "value1", "value2" };

            return "Email Sent";
        }



        // GET /Email/Template
        [HttpGet]
        [Route("template")]
        public string Template()
        {

            // Create a new parameters collection to hold our mail-merge fields:
            NameValueCollection parameters = new NameValueCollection
            {
                { "date", DateTime.Today.ToString() }
            };
            string subject;

            // We now want to parse the message templates, inserting the mail merge data and extracting the subject:
            string htmlContent = _email.ParseMessageTemplate("htmlTemplate.html", parameters, out subject);
            string textContent = _email.ParseMessageTemplate("textTemplate.txt", parameters);

            // Send the email with both html and plain text content.
            _email.Send("no-reply@youritteam.com.au", "info@youritteam.com.au", subject, htmlContent, textContent);

            return "Templated Email Sent";
        }
    }
}
