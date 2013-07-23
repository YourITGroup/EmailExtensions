using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using Umbraco.Core.Persistence;

namespace Refactored.UmbracoEmailExtensions.Models
{
    public partial class Message
    {
        [Ignore]
        public bool FollowUpOverdue
        {
            get
            {
                return FollowupDue.HasValue && !FollowedUp.HasValue && FollowupDue.Value < DateTime.Now;
            }
        }

        [Ignore]
        public bool FollowUpPending
        {
            get
            {
                return FollowupDue.HasValue && !FollowedUp.HasValue && FollowupDue.Value >= DateTime.Now;
            }
        }

        [Ignore]
        public bool RenderHtmlBody
        {
            get
            {
                return !string.IsNullOrWhiteSpace(HtmlBody);
            }
        }

        [Ignore]
        public bool RenderPlainBody
        {
            get
            {
                return !RenderHtmlBody && !string.IsNullOrWhiteSpace(PlainBody);
            }
        }

        [Ignore]
        public string SanitisedHtmlBody
        {
            get
            {
                if (RenderHtmlBody)
                {
                    try
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(HtmlBody);
                        var body = doc.DocumentNode.SelectSingleNode("//body");
                        if (body != null)
                            return body.InnerHtml;// HttpContext.Current.Server.HtmlEncode(body.InnerHtml);
                        else
                            // Already sanitised - doesn't contain a body element.
                            return HtmlBody;
                    }
                    catch { }
                }
                return null;
            }
        }


    }
}