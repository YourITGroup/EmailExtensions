using Refactored.Email.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
#if NET45
using System.Web;
#else
using System.IO;
#endif
namespace Refactored.Email.Extensions
{
    public static class EmailExtensions
    {
        /// <summary>
        /// Adds the specified addresses to the message and returns the modified message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="replyTo"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <returns></returns>
        public static MailMessage AddAddresses(this MailMessage message, string from, string to, string replyTo = null, string cc = null, string bcc = null)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentNullException(nameof(from));
            }

            message.From = FormatAddress(from);
            message.CC.AddAddresses(cc);
            message.Bcc.AddAddresses(bcc);
            message.To.AddAddresses(to);
            message.ReplyToList.AddAddresses(replyTo);

            return message;
        }

        /// <summary>
        /// Adds the specified addresses to the message and returns the modified message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <returns></returns>
        public static MailMessage AddAddresses(this MailMessage message, MailAddress from, MailAddressCollection to, MailAddressCollection replyTo = null, MailAddressCollection cc = null, MailAddressCollection bcc = null)
        {
            message.From = from.FormatAddress();
            message.CC.AddAddresses(cc);
            message.Bcc.AddAddresses(bcc);
            message.To.AddAddresses(to);
            message.ReplyToList.AddAddresses(replyTo);

            return message;
        }

        public static MailAddressCollection AddAddresses(this MailAddressCollection collection, object addresses)
        {
            if (addresses is MailAddressCollection addressCollection)
            {
                foreach (MailAddress address in addressCollection)
                    collection.Add(address);
            }
            else
            {
                if (!(addresses is string) || string.IsNullOrEmpty(addresses.ToString()))
                {
                    return collection;
                }

                string strAddresses = addresses.ToString();

                foreach (string address in strAddresses.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    collection.Add(new MailAddress(address));
                }
            }

            return collection;
        }

        public static MailAddress FormatAddress(this object address)
        {
            if (address == null)
            {
                return null;
            }

            if (address is MailAddress mailAddress)
            {
                return mailAddress;
            }
            else if (address is string)
            {
                return new MailAddress(address.ToString());
            }
            throw new ArgumentException($"{nameof(address)} must be a MailAddress or string");
        }

        /// <summary>
        /// Extension method - prepares the html and plain message body, adding attachments from within the html body as required and returns the updated message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="htmlBody"></param>
        /// <param name="plainBody"></param>
        /// <param name="emailOptions"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public static MailMessage Prepare(this MailMessage message, string htmlBody, string plainBody, EmailOptions emailOptions, IEnumerable<Attachment> attachments = null)
        {
            if (!string.IsNullOrEmpty(htmlBody))
            {
                if (!string.IsNullOrEmpty(plainBody))
                {
                    AlternateView alternateView = PrepareAlternateView(plainBody, emailOptions, "text/plain");
                    message.AlternateViews.Add(alternateView);
                }

                AlternateView alternateView1 = PrepareAlternateView(htmlBody, emailOptions, "text/html");
                message.AlternateViews.Add(alternateView1);
            }
            else
            {
                message.Body = plainBody;
                message.IsBodyHtml = false;
            }
            if (attachments != null && attachments.Any())
            {
                foreach (Attachment attachment in attachments)
                    message.Attachments.Add(attachment);
            }
            return message;
        }

        /// <summary>
        /// Prepares an alternate email view.  Email views can be html or plain text.
        /// </summary>
        /// <param name="content">Content to be prepared</param>
        /// <param name="emailOptions"></param>
        /// <param name="contentType">content type for the email content - either text/html or text/plain</param>
        /// <returns>AlternateView object containing the content provided along with it's content type.</returns>
        internal static AlternateView PrepareAlternateView(this string content, EmailOptions emailOptions, string contentType = "text/plain")
        {
            if (string.IsNullOrEmpty(content))
                return null;

            AlternateView view;
            if (string.IsNullOrEmpty(contentType))
            {
                view = AlternateView.CreateAlternateViewFromString(content);
            }
            else if (contentType == "text/html")
            {
                List<LinkedResource> resources = new List<LinkedResource>();
                content = content.ExtractFiles(resources, emailOptions).ExpandUrls(emailOptions.WebBaseUrl).SanitiseHtml();
                view = AlternateView.CreateAlternateViewFromString(content, new ContentType(contentType));
                view.ContentType.CharSet = Encoding.UTF8.WebName;
                if (!string.IsNullOrWhiteSpace(emailOptions.WebBaseUrl))
                {
                    view.BaseUri = new Uri(emailOptions.WebBaseUrl);
                }

                if (resources.Count > 0)
                {
                    foreach (LinkedResource linkedResource in resources)
                        view.LinkedResources.Add(linkedResource);
                }
            }
            else
            {
                view = AlternateView.CreateAlternateViewFromString(content, new ContentType(contentType));
            }

            return view;
        }

        /// <summary>Removes any script tags.</summary>
        /// <remarks><para>this method still has to be fully implemented.</para></remarks>
        /// <param name="content">content to be sanitised</param>
        /// <returns>sanitised content</returns>
        private static string SanitiseHtml(this string content)
        {
            return content;
        }

        /// <summary>
        /// Parses the html body for attachments.  Valid attachments are images embedded in html with src or background attributes
        /// </summary>
        /// <param name="content"></param>
        /// <param name="resources"></param>
        /// <param name="baseUrl"></param>
        /// <param name="linkWebImages"></param>
        /// <returns></returns>
        internal static string ExtractFiles(this string content, List<LinkedResource> resources, EmailOptions emailOptions)
        {
            void replaceResource(string imgType, string url)
            {
                if (string.IsNullOrEmpty(url))
                {
                    return;
                }

                LinkedResource linkedResource = null;
                if (imgType == "jpg")
                {
                    imgType = "jpeg";
                }

                string fullUrl = ReplaceUrl(url, emailOptions.WebBaseUrl);
                if (fullUrl.StartsWith("http://") || fullUrl.StartsWith("https://"))
                {
                    if (!emailOptions.LinkWebImages)
                    {
                        try
                        {
                            linkedResource = new LinkedResource(WebRequest.Create(fullUrl).GetResponse().GetResponseStream(), $"image/{imgType}");
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
#if NET45
                    HttpContext context = HttpContext.Current;
                    if (context != null)
                    {
                        fullUrl = context.Server.MapPath(fullUrl);
                    }
#elif NETSTANDARD2_0

                    fullUrl = Path.Combine(emailOptions.ContentRootPath, fullUrl);
#endif
                    try
                    {
                        linkedResource = new LinkedResource(fullUrl, $"image/{imgType}");
                    }
                    catch
                    {
                    }
                }

                if (linkedResource != null)
                {
                    string cid = Guid.NewGuid().ToString("N");
                    linkedResource.ContentId = cid;
                    resources.Add(linkedResource);
                    content = Regex.Replace(content, Regex.Escape(url), $"cid:{cid}", RegexOptions.IgnoreCase);
                }
                else
                {
                    content = Regex.Replace(content, Regex.Escape(url), fullUrl, RegexOptions.IgnoreCase);
                }
            }

            foreach (Match match in new Regex("(?<fullSrc>((src|background)=(?<quote>\"|'))(?<url>([^\"|']+)(?<imgType>jpg|jpeg|gif|png|bmp)([^\"|']*))(\\k<quote>))",
                                                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Matches(content))
            {
                string type = match.Groups["imgType"].Value;
                string url = match.Groups["url"].Value;
                string source = match.Groups["fullSrc"].Value;
                if (content.Contains(source))
                {
                    replaceResource(type, url);
                }
            }

            return content;
        }

        /// <summary>Replaces the url with a standard full url.</summary>
        /// <remarks>
        /// <para>Added the checks for /http* to take into account invalid urls formatted by TinyMCE adding '/' to the start.</para>
        /// </remarks>
        /// <param name="url"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        private static string ReplaceUrl(string url, string baseUrl)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return url;
            }

            if (url.StartsWith("~/"))
            {
                return url.Replace("~/", $"{baseUrl}/");
            }
            else if (url.StartsWith("/http:") || url.StartsWith("/https:"))
            {
                return url.Substring(1);
            }
            else if (url.StartsWith("/"))
            {
                return $"{baseUrl}{url}";
            }
            else
            {
                return $"{baseUrl}/{url}";
            }
        }
    }
}
