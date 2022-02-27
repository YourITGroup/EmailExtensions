using Refactored.Email.Configuration;
using Refactored.Email.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

/*
 * Copyright 2006-2018 Your IT Group Pty Ltd P/L
 * 
 * Contact: http://youritteam.com.au/
 * Email:   mailto:hello@youritteam.com.au
 * 
 * This code remains the property of Your IT Group Pty Ltd, and is used under non-exclusive license.
 */
[assembly: AllowPartiallyTrustedCallers]

namespace Refactored.Email
{
    /// <summary>
    /// SMTP Email Functionality includes Mail Merge and HTML/Plain text alternate views.
    /// </summary>
    /// <remarks>
    ///  <para>Copyright 2022 Your IT Group Pty Ltd </para>
    ///  <para>Email:  <a href="mailto:hello@youritteam.com.au">hello@youritteam.com.au</a> </para>
    /// </remarks>
    public class Sender
    {
        public EmailOptions Options { get; }
        public Sender(EmailOptions emailOptions)
        {
            Options = emailOptions;
        }


        /// <summary>
        /// Sends an Email with a Html body and an alternate Text body.
        /// </summary>
        /// <param name="from">From email address</param>
        /// <param name="to">To email addresses, separated by ";"</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML formatted content</param>
        /// <param name="plainBody">Plain text formatted content</param>
        /// <param name="replyTo">Reply To email addresses separated by ";"</param>
        /// <param name="cc">CC email addresses separated by ";"</param>
        /// <param name="bcc">BCC email addresses separated by ";"</param>
        /// <param name="attachments">List of attachments to add to the email</param>
        public void Send(string from, string to, string subject, string htmlBody, string plainBody, string replyTo = "", string cc = "", string bcc = "", IEnumerable<Attachment> attachments = null)
        {
            if (string.IsNullOrEmpty(htmlBody) && string.IsNullOrEmpty(plainBody))
            {
                throw new ArgumentException("Please specify a valid message for either htmlBody or plainBody.");
            }

            using (MailMessage message = new MailMessage().AddAddresses(from, to, replyTo, cc, bcc))
            {
                message.Subject = subject;
                Send(message.Prepare(htmlBody, plainBody, Options, attachments));
            }
        }

        /// <summary>
        /// Sends an Email with a Html body and an alternate Text body.
        /// </summary>
        /// <param name="from">From email address</param>
        /// <param name="to">Collection of Email Addresses to send the email to</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML formatted content</param>
        /// <param name="plainBody">Plain text formatted content</param>
        /// <param name="replyTo">Collection of Email Addresses to set as the Reply To list</param>
        /// <param name="cc">Collection of Email Addresses to copy the email to</param>
        /// <param name="bcc">Collection of Email Addresses to "Blind" copy the email to - these won't be seen by the email client.</param>
        /// <param name="attachments">Collection of attachments to add to the message</param>
        public void Send(string from, MailAddressCollection to, string subject, string htmlBody, string plainBody, MailAddressCollection replyTo = null, MailAddressCollection cc = null, MailAddressCollection bcc = null, IEnumerable<Attachment> attachments = null)
        {
            Send(new MailAddress(from), to, subject, htmlBody, plainBody, replyTo, cc, bcc, attachments);
        }

        /// <summary>
        /// Sends an Email with a Html body and an alternate Text body.
        /// </summary>
        /// <param name="from">From email address</param>
        /// <param name="to">Collection of Email Addresses to send the email to</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML formatted content</param>
        /// <param name="plainBody">Plain text formatted content</param>
        /// <param name="replyTo">Collection of Email Addresses to set as the Reply To list</param>
        /// <param name="cc">Collection of Email Addresses to copy the email to</param>
        /// <param name="bcc">Collection of Email Addresses to "Blind" copy the email to - these won't be seen by the email client.</param>
        /// <param name="attachments">Collection of attachments to add to the message</param>
        public void Send(MailAddress from, MailAddressCollection to, string subject, string htmlBody, string plainBody, MailAddressCollection replyTo = null, MailAddressCollection cc = null, MailAddressCollection bcc = null, IEnumerable<Attachment> attachments = null)
        {
            if (string.IsNullOrEmpty(htmlBody) && string.IsNullOrEmpty(plainBody))
            {
                throw new ArgumentException("Please specify a valid message for either htmlBody or plainBody.");
            }

            using (MailMessage message = new MailMessage().AddAddresses(from, to, replyTo, cc, bcc))
            {
                message.Subject = subject;
                Send(message.Prepare(htmlBody, plainBody, Options, attachments));
            }
        }

        /// <summary>
        /// Sends an already prepared MailMessage.
        /// </summary>
        /// <param name="message"></param>
        public void Send(MailMessage message)
        {
            using (var client = new SmtpClient
            {
#if NET45
                EnableSsl = Options.EnableSsl
#elif NETSTANDARD2_0
                EnableSsl = Options.EnableSsl,
                Host = Options.SmtpSettings.Host,
                Port = Options.SmtpSettings.Port,
                Credentials = Options.SmtpSettings.Credentials
#endif
            })
            {
                client.Send(message);
            }
        }

        /// <summary>
        /// Validates that a passed in string is a valid email address format.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string ValidateEmailAddress(string email)
        {
            string msg = string.Empty;
            try
            {
                MailAddress mailAddress = new MailAddress(email);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return msg;
        }

        /// <summary>
        /// Retrieves the actual template filename from application settings based on the templateName and the MailTemplateDirectory.
        /// </summary>
        /// <remarks>
        /// appSettings configuration should contain something like the following:
        /// <code>
        /// <![CDATA[
        /// 	<add key="mailTemplateDir" value="~/common/mail_templates/"/>
        /// 	<add key="mtHtmlMembershipActivated" value="membership_activated.html"/>
        /// ]]>
        /// </code>
        /// </remarks>
        /// <param name="templateName"></param>
        /// <seealso cref="P:Refactored.Email.Email.MailTemplateDirectory" />
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplate(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns></returns>
        public string GetMessageTemplate(string templateName)
        {
            string templateDirectory = Options.MailTemplateDirectory;
#if NET45
            if (!Directory.Exists(Options.MailTemplateDirectory))
            {
                templateDirectory = HttpContext.Current?.Server?.MapPath(Options.MailTemplateDirectory) ?? Options.MailTemplateDirectory;
            }
            string template = System.Configuration.ConfigurationManager.AppSettings[templateName];
#elif NETSTANDARD2_0
            if (!Directory.Exists(Options.MailTemplateDirectory))
            {
                templateDirectory = Path.Combine(Options.ContentRootPath, Options.MailTemplateDirectory);
            }
            string template = ConfigurationManager.AppSettings[templateName];
#endif

            if (string.IsNullOrEmpty(template))
            {
                template = templateName;
            }

            return Path.Combine(templateDirectory, template);
        }

        private static string ExtractSubject(string content)
        {
            string subject = string.Empty;
            Match match = new Regex("<title>(?<pageTitle>.*)</title>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline).Match(content);
            if (match.Groups.Count > 1)
            {
                subject = match.Groups["pageTitle"].Value.Trim().Replace(Environment.NewLine, " ").Replace("  ", " ");
                try
                {
#if NET45
                    subject = HttpContext.Current.Server.HtmlDecode(subject);
#elif NETSTANDARD2_0
                    subject = HttpUtility.UrlDecode(subject);
#endif
                }
                catch
                {
                }
            }
            return subject;
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" /> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="P:Refactored.Email.Email.MailTemplateDirectory" />
        /// <seealso cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" />
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplateContent(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplate<T>(string templateName, T parameters)
        {
            return ParseMessageTemplate(templateName, parameters, out _);
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" /> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="P:Refactored.Email.Email.MailTemplateDirectory" />
        /// <seealso cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" />
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplateContent(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplate<T>(string templateName, T parameters, out string subject)
        {
            return ParseMessageTemplateContent(File.ReadAllText(GetMessageTemplate(templateName)), parameters, out subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" /> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="P:Refactored.Email.Email.MailTemplateDirectory" />
        /// <seealso cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" />
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplateContent(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplate(string templateName, IDictionary<string, object> parameters)
        {
            return ParseMessageTemplate(templateName, parameters, out string subject);
        }

        /// <summary>
        /// Returns the content of the message template with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" /> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="P:Refactored.Email.Email.MailTemplateDirectory" />
        /// <seealso cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" />
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplateContent(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content of the named Template file</returns>
        public string ParseMessageTemplate(string templateName, NameValueCollection parameters)
        {
            return ParseMessageTemplate(templateName, parameters, out string subject);
        }

        /// <summary>
        /// Returns the content of the message template with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" /> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="P:Refactored.Email.Email.MailTemplateDirectory" />
        /// <seealso cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" />
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplateContent(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content of the named Template file</returns>
        public string ParseMessageTemplate(string templateName, NameValueCollection ValueParameters, out string subject)
        {
            return ParseMessageTemplate(templateName: templateName, parameters: ValueParameters, subject: out subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplate(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplateContent(string content, IDictionary<string, object> parameters)
        {
            return ParseMessageTemplateContent(content, parameters, out string subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data.  Any parameter that contains enumerated data will be rendered as a list.</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplate(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplateContent(string content, IDictionary<string, object> parameters, out string subject)
        {
            // Basic check for the existence of HTML content by checking for a <title> tag
            bool isHtml = !string.IsNullOrEmpty(ExtractSubject(content));

            foreach (string key in parameters.Keys)
            {
                object param = parameters[key];
                // If the parameter is an Enumerable then we can iterate through each item in it and build a list.
                if (!(param is string) && IsEnumerable(param.GetType()))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (object item in (IEnumerable)param)
                    {
                        if (isHtml)
                            stringBuilder.AppendFormat("<li>{0}</li>", item);
                        else
                            stringBuilder.AppendFormat("{0}{1}", item, Environment.NewLine);
                    }

                    // Replace the original param with the build list for further processing.
                    param = !isHtml ? stringBuilder.ToString() : $"<ul>{stringBuilder}</ul>";
                }
                content = content.Replace(string.Format(Options.FieldPattern, key), param.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
            }
            subject = ExtractSubject(content);

            return content;
        }

        /// <summary>
        /// Helper function to determine whether an object is an Enumerable or not.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsEnumerable(Type type)
        {
            foreach (Type type1 in type.GetInterfaces())
            {
                if (type1.IsGenericType && type1.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplate(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplateContent<T>(string content, T parameters)
        {
            return ParseMessageTemplateContent(content, parameters, out string subject);
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplate(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplateContent<T>(string content, T parameters, out string subject)
        {
            if ((object)parameters is NameValueCollection)
            {
                return ParseMessageTemplateContent(content, (object)parameters as NameValueCollection, out subject);
            }

            if (parameters is IDictionary)
            {
                return ParseMessageTemplateContent(content, (IDictionary)(object)parameters, out subject);
            }

            return ParseMessageTemplateContent(content, ((object)parameters).ToDictionary<object>(), out subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplate(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplateContent(string content, NameValueCollection parameters)
        {
            return ParseMessageTemplateContent(content, parameters, out string subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="M:Refactored.Email.Email.ParseMessageTemplate(System.String,System.Collections.Generic.IDictionary{System.String,System.Object})" />
        /// <returns>Content that has been parsed and updated</returns>
        public string ParseMessageTemplateContent(string content, NameValueCollection parameters, out string subject)
        {
            foreach (string key in parameters.Keys)
            {
                content = content.Replace(string.Format(Options.FieldPattern, key), parameters[key] ?? "", StringComparison.OrdinalIgnoreCase);
            }

            subject = ExtractSubject(content);
            return content;
        }
    }
}
