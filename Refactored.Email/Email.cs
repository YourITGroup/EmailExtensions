using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


/*
 * Copyright 2006-2011 R & E Foster & Associates P/L
 * 
 * Contact: http://refoster.com.au/
 * Email:   mailto:info@refoster.com.au
 * 
 * This code remains the property of R & E Foster & Associates, and is used under non-exclusive license.
 */
[assembly:AllowPartiallyTrustedCallers]

namespace Refactored.Email
{

    /// <summary>
    /// SMTP Email Functionality includes Mail Merge and HTML/Plain text alternate views.
    /// </summary>
    /// <remarks>
    ///  <para>Copyright 2006-2011 R &amp; E Foster &amp; Associates P/L </para>
    ///  <para>Documentation &amp; Download: <a href="http://refoster.com.au/">http://refoster.com.au/</a> </para>
    ///  <para>Email:  <a href="mailto:info@refoster.com.au">info@refoster.com.au</a> </para>
    /// </remarks>
    public static class Email
    {
        private static string fieldPattern = "{{0}}";
        private static string fieldDelimiters = "{}";

        //private static List<Attachment> attachments;

        /// <summary>
        /// Sends an Email with a Html body and an alternate Text body.
        /// </summary>
        /// <param name="from">From email address</param>
        /// <param name="to">To email addresses, separated by ";"</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML formatted content</param>
        /// <param name="plainBody">Plain text formatted content</param>
        /// <param name="cc">CC email addresses, separated by ";"</param>
        /// <param name="bcc">BCC email addresses, separated by ";"</param>
        /// <param name="attachments">List of <c ref="System.Net.Mail.Attachment">Attachments</c></param>
        public static void SendEmail(string from, string to, string subject, string htmlBody, string plainBody, string cc = "", string bcc = "",
            IEnumerable<Attachment> attachments = null)
        {
            if (string.IsNullOrEmpty(htmlBody) && string.IsNullOrEmpty(plainBody))
            {
                throw new ArgumentException("Please specify a valid message for either htmlBody or plainBody.");
            }
            
            using (MailMessage message = new MailMessage())
            {
                if (!string.IsNullOrEmpty(from))
                    message.From = new MailAddress(from);

                if (!string.IsNullOrEmpty(to))
                    foreach (string email in to.Split(';'))
                        // Set the Bcc address of the mail message
                        if (email.Trim() != string.Empty)
                            message.To.Add(new MailAddress(email));

                if (!string.IsNullOrEmpty(cc))
                    foreach (string email in cc.Split(';'))
                        // Set the Bcc address of the mail message
                        if (email.Trim() != string.Empty)
                            message.CC.Add(new MailAddress(email));

                if (!string.IsNullOrEmpty(bcc))
                    foreach (string email in bcc.Split(';'))
                        // Set the Bcc address of the mail message
                        if (email.Trim() != string.Empty)
                            message.Bcc.Add(new MailAddress(email));
                message.Subject = subject;
                message.BodyEncoding = Encoding.UTF8;
                PrepareBody(message, htmlBody, plainBody);

                if (attachments != null && attachments.Any())
                {
                    foreach (Attachment a in attachments)
                    {
                        message.Attachments.Add(a);
                    }
                }

                SendMessage(message);
            }
        }

        private static void SendMessage(MailMessage message)
        {
            SmtpClient smtpClient = new SmtpClient { EnableSsl = Email.EnableSsl };
            smtpClient.Send(message);
        }

        /// <summary>
        /// Sends an Email with a Html body and an alternate Text body.
        /// </summary>
        /// <param name="from">From email address</param>
        /// <param name="to">Collection of Email Addresses to send the email to</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlBody">HTML formatted content</param>
        /// <param name="plainBody">Plain text formatted content</param>
        /// <param name="cc">Collection of Email Addresses to copy the email to</param>
        /// <param name="bcc">Collection of Email Addresses to "Blind" copy the email to - these won't be seen by the email client.</param>
        /// <param name="attachments">List of <c ref="System.Net.Mail.Attachment">Attachments</c></param>
        public static void SendEmail(string from, MailAddressCollection to, string subject, 
            string htmlBody, string plainBody, 
            MailAddressCollection cc = null, MailAddressCollection bcc = null, 
            IEnumerable<Attachment> attachments = null)
        {
            if (string.IsNullOrEmpty(htmlBody) && string.IsNullOrEmpty(plainBody))
            {
                throw new ArgumentException("Please specify a valid message for either htmlBody or plainBody.");
            }
            using (MailMessage message = new MailMessage())
            {
                if (!string.IsNullOrEmpty(from))
                    message.From = new MailAddress(from);

                foreach (MailAddress ma in to)
                    message.To.Add(ma);
                if (cc != null)
                    foreach (MailAddress ma in cc)
                        message.CC.Add(ma);
                if (bcc != null)
                    foreach (MailAddress ma in bcc)
                        message.Bcc.Add(ma);
                message.Subject = subject;
                message.BodyEncoding = Encoding.UTF8;
                PrepareBody(message, htmlBody, plainBody);
                if (attachments != null && attachments.Any())
                {
                    foreach (Attachment a in attachments)
                    {
                        message.Attachments.Add(a);
                    }
                }
                SendMessage(message);
            }
        }

        /// <summary>
        /// Prepares a plain text email view.
        /// </summary>
        /// <param name="content">Plain Text content</param>
        /// <returns>AlternateView object containing plain text content</returns>
        public static AlternateView PrepareAlternateView(string content)
        {
            return PrepareAlternateView(content, "text/plain");
        }

        /// <summary>
        /// Prepares an alternate email view.  Email views can be html or plain text.
        /// </summary>
        /// <param name="content">Content to be prepared</param>
        /// <param name="contentType">content type for the email content - either text/html or text/plain</param>
        /// <returns>AlternateView object containing the content provided along with it's content type.</returns>
        public static AlternateView PrepareAlternateView(string content, string contentType)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            AlternateView view = null;

            if (string.IsNullOrEmpty(contentType))
                view = AlternateView.CreateAlternateViewFromString(content);
            else
            {
                if (contentType.Contains("html"))
                {
                    List<LinkedResource> resources = new List<LinkedResource>();

                    // If the content contains references to files, then retrieve the files and attach them as LinkedContent.
                    content = content.ExtractFiles(resources).ExpandUrls().SanitiseHtml();

                    view = AlternateView.CreateAlternateViewFromString(content, Encoding.UTF8, contentType);
                    view.BaseUri = new Uri(WebBaseUrl);

                    if (resources.Count > 0)
                        foreach (LinkedResource r in resources)
                        {
                            view.LinkedResources.Add(r);
                        }

                }
                else
                {
                    view = AlternateView.CreateAlternateViewFromString(content,
                                Encoding.UTF8, contentType);
                }
            }

            return view;
        }

        /// <summary>
        /// Removes any script tags.
        /// </summary>
        /// <remarks><para>this method still has to be fully implemented.</para></remarks>
        /// <param name="content">content to be sanitised</param>
        /// <returns>sanitised content</returns>
        private static string SanitiseHtml(this string content)
        {
            //Regex reg = new Regex(@"^(<script>)(.*?\.)</script>$", RegexOptions.Multiline);
            //MatchCollection mc = reg.Matches(content);
            //foreach (Match m in mc)
            //{
            //    // TODO: Finish this by replacing matches.
            //}

            return content;
        }

        /// <summary>
        /// Replaces the url with a standard full url.
        /// </summary>
        /// <remarks>
        /// <para>Added the checks for /http* to take into account invalid urls formatted by TinyMCE adding '/' to the start.</para>
        /// </remarks>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string ReplaceUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;
            else
            {
                if (url.StartsWith("~/"))
                    url = url.Replace("~/", WebBaseUrl + "/");
                else if (url.StartsWith("/http:"))
                    url = url.Substring(1);
                else if (url.StartsWith("/https:"))
                    url = url.Substring(1);
                else if (url.StartsWith("/"))
                    url = WebBaseUrl + url;
                else
                    url = String.Format("{0}/{1}", WebBaseUrl, url);
            }

            return url;

        }
        /// <summary>
        /// Expands out local urls (those not containing any server/scheme parts) to full urls.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string ExpandUrls(this string content)
        {
            // We need to get the full href line so that we can compare in case of duplicate urls in the content.
            // If the url is in the content more than once and is modified, we would end up with multiple modifications on the urls
            //Regex reg = new Regex(@"(?<=href=(?<quote>""|'))(?<url>[^""|']+)(?=\k<quote>)");
            Regex reg = new Regex(@"(?<fullHref>(href=(?<quote>""|'))(?<url>[^""|']+)(\k<quote>))");
            MatchCollection mc = reg.Matches(content);
            foreach (Match m in mc)
            {
                string url = m.Groups["url"].Value;
                string fullHref = m.Groups["fullHref"].Value;
                string newUrl = ReplaceUrl(url);
                string newHref = fullHref.Replace(url, newUrl);

                // The content may not contain the url any longer if the same url is used more than once, so just skip it.
                if (!content.Contains(fullHref) || url == newUrl)
                    continue;

                content = Regex.Replace(content, Regex.Escape(fullHref), newHref, RegexOptions.IgnoreCase);
                
                // Debugging
                //content += "<br />{Old Url: " + url + "; New Url: " + newUrl + "}";
            }
            return content;
        }

        /// <summary>
        /// Parses the html body for attachments.  Valid attachments are images embedded in html with src or background attributes
        /// </summary>
        /// <param name="content"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        private static string ExtractFiles(this string content, List<LinkedResource> resources)
        {
            var context = HttpContext.Current;

            Action<string, string> replaceUrl = (imgType, url) =>
            {
                if (!string.IsNullOrEmpty(url))
                {
                    LinkedResource r = null;

                    // Make sure the ContentType is going to be correct.
                    if (imgType == "jpg")
                        imgType = "jpeg";

                    string newUrl = ReplaceUrl(url);

                    if (newUrl.StartsWith("http://") || newUrl.StartsWith("https://"))
                    {
                        if (!LinkWebImages)
                        {
                            WebRequest request = HttpWebRequest.Create(newUrl);
                            Stream srcStream = request.GetResponse().GetResponseStream();
                            r = new LinkedResource(srcStream, string.Format("image/{0}", imgType));
                        }
                    }
                    else if (context != null) // TODO: Rethink this - at the moment, it will never be executed.
                    {
                        // Assume url is relative to current script execution path.
                        newUrl = context.Server.MapPath(newUrl);

                        r = new LinkedResource(newUrl, string.Format("image/{0}", imgType));
                    }

                    if (r != null)
                    {
                        string cid = Guid.NewGuid().ToString("N");
                        r.ContentId = cid;
                        resources.Add(r);

                        // we need to update the body text by replacing the url with the cid.
                        content = Regex.Replace(content, Regex.Escape(url), string.Format("cid:{0}", cid), RegexOptions.IgnoreCase);  //content.Replace(url, string.Format("cid:{0}", cid));
                    }
                    else
                    {
                        content = Regex.Replace(content, Regex.Escape(url), newUrl, RegexOptions.IgnoreCase);  //content.Replace(url, modUrl);
                    }

                }
            };

            //string strRegex = @"(src|background)=(?<quote>""|')(.*?\.)(jpg|jpeg|gif|png|bmp)\k<quote>";
            string strRegex = @"(?<fullSrc>((src|background)=(?<quote>""|'))(?<url>([^""|']+)(?<imgType>jpg|jpeg|gif|png|bmp)([^""|']*))(\k<quote>))";
            Regex reg = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            MatchCollection mc = reg.Matches(content);

            foreach (Match m in mc)
            {
                string imgType = m.Groups["imgType"].Value;
                string url = m.Groups["url"].Value;
                string fullSrc = m.Groups["fullSrc"].Value;

                // The content may not contain the url any longer if the same image is used more than once, so just skip it.
                if (!content.Contains(fullSrc))
                    continue;

                replaceUrl(imgType, url);
                
            }

            return content;
        }

        private static void PrepareBody(MailMessage message, string htmlBody, string plainBody)
        {
            if (!string.IsNullOrEmpty(htmlBody))
            {
                if (!string.IsNullOrEmpty(plainBody))
                {
                    AlternateView textView = PrepareAlternateView(plainBody);
                    message.AlternateViews.Add(textView);

                }
                AlternateView htmlView = PrepareAlternateView(htmlBody, "text/html");

                message.AlternateViews.Add(htmlView);

            }
            else
            {
                message.Body = plainBody;
                message.IsBodyHtml = false;
            }
            //if (Attachments.Count > 0)
            //{
                //foreach (Attachment a in Attachments)
                //{
                //    message.Attachments.Add(a);
                //}
            //}
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
                // All we're doing here is trying to create an email address from the string.  If it is succesful, then we're ok.
                new MailAddress(email);
            }
            catch (Exception e)
            {
                msg = e.Message;
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
        /// <seealso cref="MailTemplateDirectory"/>
        /// <seealso cref="ParseMessageTemplate"/>
        /// <returns></returns>
        public static string GetMessageTemplate(string templateName)
        {
            string messageDir = Email.MailTemplateDirectory;
            HttpContext context = HttpContext.Current;
            messageDir = context.Server.MapPath(messageDir);
            string template = ConfigurationManager.AppSettings[templateName];
            if (string.IsNullOrEmpty(template))
                template = templateName;

            if (messageDir[messageDir.Length - 1] != Path.DirectorySeparatorChar && template[0] != Path.DirectorySeparatorChar)
                return string.Format("{0}{1}{2}", messageDir, Path.DirectorySeparatorChar, template);
            else
                return string.Format("{0}{1}", messageDir, template);
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="GetMessageTemplate"/> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="MailTemplateDirectory"/>
        /// <seealso cref="GetMessageTemplate"/>
        /// <seealso cref="ParseMessageTemplateContent"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplate<T>(string templateName, T parameters)
        {
            string subject = string.Empty;

            return ParseMessageTemplate(templateName, parameters, out subject);
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="GetMessageTemplate"/> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="MailTemplateDirectory"/>
        /// <seealso cref="GetMessageTemplate"/>
        /// <seealso cref="ParseMessageTemplateContent"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplate<T>(string templateName, T parameters, out string subject)
        {
            string content = File.ReadAllText(GetMessageTemplate(templateName));
            return ParseMessageTemplateContent(content, parameters, out subject);
        }

        ///// <summary>
        ///// Returns the content of the message template with mail merged parameters
        ///// </summary>
        ///// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="GetMessageTemplate"/> for more information</param>
        ///// <param name="parameters">Collection of field values containing mail merge data</param>
        ///// <seealso cref="MailTemplateDirectory"/>
        ///// <seealso cref="GetMessageTemplate"/>
        ///// <seealso cref="ParseMessageTemplateContent"/>
        ///// <returns>Content of the named Template file</returns>
        //public static string ParseMessageTemplate(string templateName, IOrderedDictionary parameters)
        //{
        //    string subject = string.Empty;
        //    return ParseMessageTemplate(templateName, parameters, out subject);
        //}

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="GetMessageTemplate"/> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="MailTemplateDirectory"/>
        /// <seealso cref="GetMessageTemplate"/>
        /// <seealso cref="ParseMessageTemplateContent"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplate(string templateName, IDictionary<string, object> parameters)
        {
            string subject = string.Empty;
            return ParseMessageTemplate(templateName, parameters, out subject);
        }

        /// <summary>
        /// Returns the content of the message template with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="GetMessageTemplate"/> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="MailTemplateDirectory"/>
        /// <seealso cref="GetMessageTemplate"/>
        /// <seealso cref="ParseMessageTemplateContent"/>
        /// <returns>Content of the named Template file</returns>
        public static string ParseMessageTemplate(string templateName, NameValueCollection parameters)
        {
            string subject = string.Empty;
            return ParseMessageTemplate(templateName, parameters, out subject);
        }

        /// <summary>
        /// Returns the content of the message template with mail merged parameters
        /// </summary>
        /// <param name="templateName">Name of the template registered in appSettings configuration - refer to <see cref="GetMessageTemplate"/> for more information</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="MailTemplateDirectory"/>
        /// <seealso cref="GetMessageTemplate"/>
        /// <seealso cref="ParseMessageTemplateContent"/>
        /// <returns>Content of the named Template file</returns>
        public static string ParseMessageTemplate(string templateName, NameValueCollection parameters, out string subject)
        {
            return ParseMessageTemplate(templateName, parameters, out subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="ParseMessageTemplate"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplateContent(string content, IDictionary<string, object> parameters)
        {
            string subject = string.Empty;
            return ParseMessageTemplateContent(content, parameters, out subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="ParseMessageTemplate"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplateContent(string content, IDictionary<string, object> parameters, out string subject)
        {
            NameValueCollection p = new NameValueCollection(parameters.Count);
            foreach (var key in parameters.Keys)
                p.Add(key, parameters[key].ToString());
            return ParseMessageTemplateContent(content, p, out subject);
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="ParseMessageTemplate"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplateContent<T>(string content, T parameters)
        {
            string subject = string.Empty;
            return ParseMessageTemplateContent(content, parameters, out subject);
        }

        /// <summary>
        /// Generic method to return the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="ParseMessageTemplate"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplateContent<T>(string content, T parameters, out string subject)
        {
            if (parameters is NameValueCollection)
                return ParseMessageTemplateContent(content, parameters as NameValueCollection, out subject);
            else if (parameters is IDictionary)
                return ParseMessageTemplateContent(content, (IDictionary)parameters, out subject);

            return ParseMessageTemplateContent(content, parameters.ToDictionary<object>(), out subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <seealso cref="ParseMessageTemplate"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplateContent(string content, NameValueCollection parameters)
        {
            string subject = string.Empty;
            return ParseMessageTemplateContent(content, parameters, out subject);
        }

        /// <summary>
        /// Returns the content of the message with mail merged parameters
        /// </summary>
        /// <param name="content">message body to be merged with with mail merge data</param>
        /// <param name="parameters">Collection of field values containing mail merge data</param>
        /// <param name="subject">Retrieves the subject of the email from the title if present (looks for <code>&lt;title&gt;&lt;/title&gt;</code> tags)</param>
        /// <seealso cref="ParseMessageTemplate"/>
        /// <returns>Content that has been parsed and updated</returns>
        public static string ParseMessageTemplateContent(string content, NameValueCollection parameters, out string subject)
        {
            foreach (string key in parameters.Keys)
            {
                // Handle both upper and lower case keys
                content = content.Replace(string.Format(FieldPattern, key), parameters[key] ?? "", StringComparison.OrdinalIgnoreCase);// Regex.Replace(content, Regex.Escape(string.Format(FieldPattern, key)), parameters[key] ?? "", RegexOptions.IgnoreCase);
            }

            // Try and work out the subject from the Title if this is an html email.
            Regex reg = new Regex(@"<title>(?<pageTitle>.*)</title>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
            Match m = reg.Match(content);
            if (m.Groups.Count > 1)
            {
                subject = m.Groups["pageTitle"].Value.Trim().Replace(Environment.NewLine, " ").Replace("  ", " ");
                try
                {
                    subject = HttpContext.Current.Server.HtmlDecode(subject);
                }
                catch { }
            }
            else
                subject = string.Empty;

            
            return content;
        }

        /// <summary>
        /// Gets or Sets the base filesystem directory containing mailmerge templates
        /// </summary>
        /// <seealso cref="GetMessageTemplate"/>
        public static string MailTemplateDirectory { get; set; }

        /// <summary>
        /// Gets or Sets the Http/Https base Url for relative links found in the content templates
        /// </summary>
        /// <seealso cref="LinkWebImages"/>
        public static string WebBaseUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    var context = HttpContext.Current;
                    if (context != null)
                        _baseUrl = context.Request.Url.GetLeftPart(UriPartial.Authority);
                }
                return _baseUrl;
            }
            set
            {
                _baseUrl = value;
            }
        }

        private static string _baseUrl;

        /// <summary>
        /// Enable Linking to Images instead of embedding them
        /// </summary>
        /// <remarks>Any image that has a full url will be linked to instead of embedded in the HTML email.  
        /// Images with a relative url will can be linked as well if WebBaseUrl is set.
        /// </remarks>
        /// <seealso cref="WebBaseUrl"/>
        public static bool LinkWebImages { get; set; }

        /// <summary>
        /// Gets or Sets the field pattern for mail merge templates.
        /// </summary>
        /// <remarks>The default FieldPattern is {{0}}.  Suggested alternatives are &lt;%{0}%&gt; or [{0}].  Note that the field pattern must contain {0}.</remarks>
        public static string FieldPattern
        {
            get { return fieldPattern; }
            set { fieldPattern = value; }
        }

        /// <summary>
        /// Gets or Sets basic field delimiters used when creating email templates.
        /// </summary>
        /// <remarks>
        ///   Specify the Field delimiters used when creating email templates.  This sets the Field Pattern up based on the following rules:
        ///       If a single delimiter character is specified, it will be assumed to have been placed at the start of the and end of the field.
        ///       Otherwise, two delimiter characters will be split so that the first is at the start and the second is at the end.
        ///  </remarks>
        public static string FieldDelimiters
        {
            get { return fieldDelimiters; }
            set { 
                if (string.IsNullOrEmpty(value) || value.Trim() == "")
                    return;
                fieldDelimiters = value;
                if (fieldDelimiters.Length == 1)
                    FieldPattern = String.Format("{0}{{0}}{0}", value);
                else
                    FieldPattern = String.Format("{0}{{0}}{1}", value[0], value[1]);
            }
        }


        /// <summary>
        /// Enables or Disables the SSL protocol.  Disabled by default.
        /// </summary>
        public static bool EnableSsl{get;set;}

        ///// <summary>
        ///// Gets the list of Attachments generated by parsing a HTML body.  Includes all embedded images
        ///// </summary>
        //public static List<Attachment> Attachments
        //{
        //    get
        //    {
        //        if (attachments == null)
        //            attachments = new List<Attachment>();
        //        return attachments;
        //    }
        //}

    }
}

