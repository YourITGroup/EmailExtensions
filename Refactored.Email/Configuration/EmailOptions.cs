using System;
using System.Collections.Generic;
using System.Web;

namespace Refactored.Email.Configuration
{
    public class EmailOptions
    {
        private static string _fieldDelimiters = "{}";
        private static string _baseUrl;

        /// <summary>Enable Linking to Images instead of embedding them</summary>
        /// <remarks>Any image that has a full url will be linked to instead of embedded in the HTML email.
        /// Images with a relative url will can be linked as well if WebBaseUrl is set.
        /// </remarks>
        /// <seealso cref="P:Refactored.Email.Email.WebBaseUrl" />
        public bool LinkWebImages { get; set; } = true;

        /// <summary>
        /// Gets or Sets the field pattern for mail merge templates.
        /// </summary>
        /// <remarks>The default FieldPattern is {{0}}.  Suggested alternatives are &lt;%{0}%&gt; or [{0}].  Note that the field pattern must contain {0}.</remarks>
        public string FieldPattern { get; set; } = "{{0}}";

        /// <summary>
        /// Gets a list of transforms to be applied to Message content fields.
        /// </summary>
        /// <remarks>
        /// <para>A Field Transform is applied to each item in the message parameters to attempt to match it to a fields in the message.  
        /// For Example, a Field Transform of "_" mapped to "-" will match message parameters `Field-One` for a key of `Field_One`.
        /// </para>
        /// </remarks>
        public Dictionary<string, string> FieldTransforms { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Convert new lines (/n/r) to breaks in Html Templates.
        /// </summary>
        public bool ConvertNewlineToBreak { get; set; } = false;

        /// <summary>
        /// Gets or Sets basic field delimiters used when creating email templates.
        /// </summary>
        /// <remarks>
        ///  Specify the Field delimiters used when creating email templates.  This sets the Field Pattern up based on the following rules:
        ///      If a single delimiter character is specified, it will be assumed to have been placed at the start and end of the field.
        ///      Otherwise, two delimiter characters will be split so that the first is at the start and the second is at the end.
        /// </remarks>
        public string FieldDelimiters
        {
            get => _fieldDelimiters;
            set
            {
                if (string.IsNullOrEmpty(value) || value.Trim() == "")
                {
                    return;
                }

                _fieldDelimiters = value;
                if (_fieldDelimiters.Length == 1)
                {
                    FieldPattern = $"{value}{{0}}{value}";
                }
                else
                {
                    FieldPattern = $"{value[0]}{{0}}{value[1]}";
                }
            }
        }

        /// <summary>
        /// Enables or Disables the SSL protocol.  Disabled by default.
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or Sets the base filesystem directory containing mailmerge templates
        /// </summary>
        /// <seealso cref="M:Refactored.Email.Email.GetMessageTemplate(System.String)" />
        public string MailTemplateDirectory { get; set; }

        /// <summary>
        /// Gets or Sets the Http/Https base Url for relative links found in the content templates
        /// </summary>
        /// <seealso cref="P:Refactored.Email.Email.LinkWebImages" />
        public string WebBaseUrl
        {
            get
            {
#if NET45
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    HttpContext current = HttpContext.Current;
                    if (current != null)
                        _baseUrl = current.Request.Url.GetLeftPart(UriPartial.Authority);
                }
#endif

                return _baseUrl;
            }
            set => _baseUrl = value;
        }

#if NETSTANDARD2_0
        public string ContentRootPath { get; set; }

        public SmtpSection SmtpSettings { get; set; }
#endif
    }
}
