using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core.IO;

namespace Refactored.UmbracoEmailExtensions.Config
{
    public class Configuration
    {
        private static bool init;

        private static int emailWrapperHtmlContentId;
        private static int emailWrapperTextContentId;
        private static int tinyMceDataTypeId;
        private static string richTextConfig;

        public static int EmailWrapperHtmlContentId
        {
            get
            {
                if (!init)
                    Init();
                return emailWrapperHtmlContentId;
            }
        }

        public static int EmailWrapperTextContentId
        {
            get
            {
                if (!init)
                    Init();
                return emailWrapperTextContentId;
            }
        }

        public static int TinyMceDataTypeId
        {
            get
            {
                if (!init)
                    Init();
                return tinyMceDataTypeId;
            }
        }

        public static string RichTextConfig
        {
            get
            {
                if (!init)
                    Init();
                return richTextConfig;
            }
        }

        private static void Init()
        {
            // Load config
            XmlDocument xd = new XmlDocument();
            xd.Load(IOHelper.MapPath(SystemDirectories.Config + "/Refactored.UmbracoEmailExtensions.config"));

            emailWrapperHtmlContentId = 0;
            var node = xd.SelectSingleNode("descendant::Email/WrapperHtmlContentId");
            if (node != null)
                int.TryParse(node.InnerText, out emailWrapperHtmlContentId);

            emailWrapperTextContentId = 0;            
            node = xd.SelectSingleNode("descendant::Email/WrapperTextContentId");
            if (node != null)
                int.TryParse(node.InnerText, out emailWrapperTextContentId);

            tinyMceDataTypeId = 0;
            node = xd.SelectSingleNode("descendant::Richtext/TinyMceDataTypeId");
            if (node != null)
                int.TryParse(node.InnerText, out tinyMceDataTypeId);

            node = xd.SelectSingleNode("descendant::Richtext/TinyMceDataTypeId");
            if (node != null)
                richTextConfig = node.InnerText;

            init = true;
        }
    }
}