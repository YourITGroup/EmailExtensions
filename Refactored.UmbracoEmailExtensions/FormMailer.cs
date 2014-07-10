using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Refactored.UmbracoEmailExtensions.Interfaces;
using Refactored.UmbracoEmailExtensions.Models;
using umbraco.NodeFactory;
using global::Umbraco.Web;
using global::Umbraco.Core;
using global::Umbraco.Core.Models;
using global::Umbraco.Core.IO;
using System.Net.Mail;
using System.Net.Mime;

namespace Refactored.UmbracoEmailExtensions
{
    public static class FormMailer
    {
        internal static object attachmentLock = new object();

        private static bool SendEmail(string fromEmail, string toEmail, string subject, string htmlBody, string textBody, string bccList = null, IEnumerable<IPublishedContent> attachments = null)
        {
            if (attachments != null && attachments.Any())
            {
                lock (attachmentLock)
                {
                    Refactored.Email.Email.SendEmail(fromEmail, toEmail, subject, htmlBody, textBody, bcc: bccList, attachments: ProcessFiles(attachments));
                }
            }
            else
            {
                Refactored.Email.Email.SendEmail(fromEmail, toEmail, subject, htmlBody, textBody, bcc: bccList);
            }
            return true;
        }

        public static void SendFormData(this UmbracoHelper context, object form, bool saveMessage = true, IEnumerable<IPublishedContent> attachments = null)
        {
            ISubmitDetails details = form as ISubmitDetails;
            //if (details == null) throw new ArgumentException("form must implement ISubmitDetails", "form");

            Dictionary<object, object> contextItems = getCurrentContextItems();
            HttpContext Context = HttpContext.Current;
            IConfirmationDetails confirm = form as IConfirmationDetails;

            try
            {
                if (!string.IsNullOrEmpty(details.FieldPattern))
                {
                    if (details.FieldPattern.Contains("{0}"))
                        Refactored.Email.Email.FieldPattern = details.FieldPattern;
                    else
                        Refactored.Email.Email.FieldDelimiters = details.FieldPattern;
                }
                else
                    Refactored.Email.Email.FieldDelimiters = "[]";

                if (details != null && details.HtmlTemplateId > 0 && details.TextTemplateId > 0)
                {
                    string subject = string.Empty;
                    string htmlBody = string.Empty;
                    string textBody = string.Empty;
                    if (details.HtmlTemplateId > 0)
                    {
                        htmlBody = Refactored.Email.Email.ParseMessageTemplateContent(
                            context.RenderTemplate(details.HtmlTemplateId).ToString(),
                            form, out subject);
                    }

                    if (details.TextTemplateId > 0)
                    {
                        textBody = Refactored.Email.Email.ParseMessageTemplateContent(
                            context.RenderTemplate(details.TextTemplateId).ToString(),
                            form);

                        if (string.IsNullOrEmpty(subject))
                            subject = new Node(details.TextTemplateId).Name;
                    }


                    SendEmail(details.FromEmail, details.ToEmail, subject, htmlBody, textBody, details.BccEmail, attachments);
                    if (saveMessage)
                        MessageManager.CreateMessage(confirm == null ? details.FromEmail : confirm.SubmitterEmail, details.ToEmail, subject, htmlBody, textBody);
                }

                if (confirm != null && (confirm.HtmlConfirmationTemplateId > 0 || confirm.TextConfirmationTemplateId > 0))
                {
                    string htmlBody = string.Empty;
                    string textBody = string.Empty;
                    string subject = string.Empty;

                    if (confirm.HtmlConfirmationTemplateId > 0)
                    {
                        htmlBody = Refactored.Email.Email.ParseMessageTemplateContent(
                            context.RenderTemplate(confirm.HtmlConfirmationTemplateId).ToString(),
                            form, out subject);
                    }

                    if (confirm.TextConfirmationTemplateId > 0)
                    {
                        textBody = Refactored.Email.Email.ParseMessageTemplateContent(
                            context.RenderTemplate(confirm.TextConfirmationTemplateId).ToString(),
                            form);

                        if (string.IsNullOrEmpty(subject))
                            subject = new Node(confirm.TextConfirmationTemplateId).Name;
                    }

                    SendEmail(confirm.FromEmail, confirm.SubmitterEmail, subject, htmlBody, textBody, attachments: attachments);
                    //Refactored.Email.Email.SendEmail(confirm.FromEmail, confirm.SubmitterEmail, subject, htmlBody, textBody);
                }
            }
            finally
            {
                updateLocalContextItems(contextItems, Context);
            }
        }

        public static void SendFormData(this UmbracoHelper context, object form, NameValueCollection formData, bool saveMessage = true, IEnumerable<IPublishedContent> attachments = null)
        {
            ISubmitDetails details = form as ISubmitDetails;
            if (details == null) throw new ArgumentException("form must implement ISubmitDetails", "form");

            Dictionary<object, object> contextItems = getCurrentContextItems();
            HttpContext Context = HttpContext.Current;
            IConfirmationDetails confirm = form as IConfirmationDetails;

            try
            {
                if (!string.IsNullOrEmpty(details.FieldPattern))
                {
                    if (details.FieldPattern.Contains("{0}"))
                        Refactored.Email.Email.FieldPattern = details.FieldPattern;
                    else
                        Refactored.Email.Email.FieldDelimiters = details.FieldPattern;
                }
                else
                    Refactored.Email.Email.FieldDelimiters = "[]";

                string subject = string.Empty;
                string htmlBody = string.Empty;
                string textBody = string.Empty;
                if (details.HtmlTemplateId > 0)
                {
                    htmlBody = Refactored.Email.Email.ParseMessageTemplateContent(
                        context.RenderTemplate(details.HtmlTemplateId).ToString(),
                        formData, out subject);
                }

                if (details.TextTemplateId > 0)
                {
                    textBody = Refactored.Email.Email.ParseMessageTemplateContent(
                        context.RenderTemplate(details.TextTemplateId).ToString(),
                        formData);

                    if (string.IsNullOrEmpty(subject))
                        subject = new Node(details.TextTemplateId).Name;
                }

                SendEmail(details.FromEmail, details.ToEmail, subject, htmlBody, textBody, bccList: details.BccEmail, attachments: attachments);
                //Refactored.Email.Email.SendEmail(details.FromEmail, details.ToEmail, subject, htmlBody, textBody, bcc: details.BccEmail);

                if (saveMessage)
                    MessageManager.CreateMessage(confirm == null ? details.FromEmail : confirm.SubmitterEmail, details.ToEmail, subject, htmlBody, textBody);

                if (confirm != null && (confirm.HtmlConfirmationTemplateId > 0 || confirm.TextConfirmationTemplateId > 0))
                {
                    htmlBody = string.Empty;
                    textBody = string.Empty;
                    if (confirm.HtmlConfirmationTemplateId > 0)
                    {
                        htmlBody = Refactored.Email.Email.ParseMessageTemplateContent(
                            context.RenderTemplate(confirm.HtmlConfirmationTemplateId).ToString(),
                            formData, out subject);
                    }

                    if (confirm.TextConfirmationTemplateId > 0)
                    {
                        textBody = Refactored.Email.Email.ParseMessageTemplateContent(
                            context.RenderTemplate(confirm.TextConfirmationTemplateId).ToString(),
                            formData);

                        if (string.IsNullOrEmpty(subject))
                            subject = new Node(confirm.TextConfirmationTemplateId).Name;
                    }

                    SendEmail(confirm.FromEmail, confirm.SubmitterEmail, subject, htmlBody, textBody, attachments: attachments);
                    //Refactored.Email.Email.SendEmail(details.FromEmail, confirm.SubmitterEmail, subject, htmlBody, textBody);


                }
            }
            finally
            {
                updateLocalContextItems(contextItems, Context);
            }
        }

        private static IEnumerable<Attachment> ProcessFiles(IEnumerable<IPublishedContent> attachments)
        {
            if (attachments != null && attachments.Any())
            {
                Func<string, Attachment> attach = (file) =>
                {
                    if (string.IsNullOrWhiteSpace(file))
                        return null;

                    var attachment = new Attachment(file, MediaTypeNames.Application.Octet);
                    ContentDisposition disposition = attachment.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(file);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(file);

                    return attachment;

                };
                // a.HasValue(fileAlias) - Breaking change between 6.0 and 7.1 somewhere - moved the extensions library from Umbraco.Core to Umbraco.Web.
                string fileAlias = "umbracoFile"; // TODO: Use Umbraco Constants where available (not in our dependent version of Umbraco).
                return attachments.Where(a => 
                                        a != null // Deleted Items will be null - this is a safeguard.
                                        && !string.IsNullOrWhiteSpace(a.GetPropertyValue<string>(fileAlias)))
                                    .Select(a => attach(IOHelper.MapPath(a.GetPropertyValue<string>(fileAlias))));
            }
            return null;
        }

        [Obsolete]
        public static void SendFormData(object form, NameValueCollection formData, bool saveMessage = true)
        {
            ISubmitDetails details = form as ISubmitDetails;
            if (details == null)
                throw new ArgumentException("form must implement ISubmitDetails", "form");

            Dictionary<object, object> items = getCurrentContextItems();
            HttpContext Context = HttpContext.Current;
            IConfirmationDetails confirm = form as IConfirmationDetails;

            try
            {
                if (!string.IsNullOrEmpty(details.FieldPattern))
                {
                    if (details.FieldPattern.Contains("{0}"))
                        Refactored.Email.Email.FieldPattern = details.FieldPattern;
                    else
                        Refactored.Email.Email.FieldDelimiters = details.FieldPattern;
                }
                else
                    Refactored.Email.Email.FieldDelimiters = "[]";

                string subject = string.Empty;
                string htmlBody = string.Empty;
                string textBody = string.Empty;
                if (details.HtmlTemplateId > 0)
                {
                    htmlBody = Refactored.Email.Email.ParseMessageTemplateContent(
                        umbraco.library.RenderTemplate(details.HtmlTemplateId).ToString(),
                        formData, out subject);
                }

                if (details.TextTemplateId > 0)
                {
                    textBody = Refactored.Email.Email.ParseMessageTemplateContent(
                        umbraco.library.RenderTemplate(details.TextTemplateId).ToString(),
                        formData);

                    if (string.IsNullOrEmpty(subject))
                        subject = new Node(details.TextTemplateId).Name;
                }


                Refactored.Email.Email.SendEmail(details.FromEmail, details.ToEmail, subject, htmlBody, textBody, bcc: details.BccEmail);
                if (saveMessage)
                    MessageManager.CreateMessage(confirm == null ? details.FromEmail : confirm.SubmitterEmail, details.ToEmail, subject, htmlBody, textBody);

                if (confirm != null)
                {
                    if (confirm.HtmlConfirmationTemplateId > 0 || confirm.TextConfirmationTemplateId > 0)
                    {
                        htmlBody = string.Empty;
                        textBody = string.Empty;
                        if (confirm.HtmlConfirmationTemplateId > 0)
                        {
                            htmlBody = Refactored.Email.Email.ParseMessageTemplateContent(
                                umbraco.library.RenderTemplate(confirm.HtmlConfirmationTemplateId).ToString(),
                                formData, out subject);
                        }

                        if (confirm.TextConfirmationTemplateId > 0)
                        {
                            textBody = Refactored.Email.Email.ParseMessageTemplateContent(
                                umbraco.library.RenderTemplate(confirm.TextConfirmationTemplateId).ToString(),
                                formData);

                            if (string.IsNullOrEmpty(subject))
                                subject = new Node(confirm.TextConfirmationTemplateId).Name;
                        }

                        Refactored.Email.Email.SendEmail(details.FromEmail, confirm.SubmitterEmail, subject, htmlBody, textBody);
                    }

                }
            }
            finally
            {
                updateLocalContextItems(items, Context);
            }
        }

        private static Dictionary<object, object> getCurrentContextItems()
        {
            IDictionary items = HttpContext.Current.Items;
            Dictionary<object, object> currentItems = new Dictionary<object, object>();
            IDictionaryEnumerator ide = items.GetEnumerator();
            while (ide.MoveNext())
            {
                currentItems.Add(ide.Key, ide.Value);
            }
            return currentItems;
        }

        private static void updateLocalContextItems(IDictionary items, HttpContext Context)
        {
            Context.Items.Clear();
            IDictionaryEnumerator ide = items.GetEnumerator();
            while (ide.MoveNext())
            {
                Context.Items.Add(ide.Key, ide.Value);
            }
        }
    }
}
