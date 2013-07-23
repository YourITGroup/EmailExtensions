using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Refactored.UmbracoEmailExtensions.Interfaces;
using Refactored.UmbracoEmailExtensions.Models;
using umbraco.NodeFactory;
using Umbraco.Web;

namespace Refactored.UmbracoEmailExtensions
{
    public static class FormMailer
    {
        public static void SendFormData(this UmbracoHelper context, object form, bool saveMessage = true)
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

                        Refactored.Email.Email.SendEmail(details.FromEmail, confirm.SubmitterEmail, subject, htmlBody, textBody);
                    }

                }
            }
            finally
            {
                updateLocalContextItems(contextItems, Context);
            }
        }

        public static void SendFormData(this UmbracoHelper context, object form, NameValueCollection formData, bool saveMessage = true)
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

                        Refactored.Email.Email.SendEmail(details.FromEmail, confirm.SubmitterEmail, subject, htmlBody, textBody);
                    }

                }
            }
            finally
            {
                updateLocalContextItems(contextItems, Context);
            }
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
