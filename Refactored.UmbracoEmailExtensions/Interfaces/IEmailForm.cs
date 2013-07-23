using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Refactored.UmbracoEmailExtensions.Interfaces
{
    [Obsolete("Use ISubmitDetails and IConfirmationDetails instead")]
    public interface IEmailForm
    {
        string ToEmail { get;  }
        string FromEmail { get; }
        string BccEmail { get; }
        string SubmitterEmail { get; }
        int HtmlTemplateId { get; }
        int HtmlConfirmationTemplateId { get; }
        int TextTemplateId { get; }
        int TextConfirmationTemplateId { get; }
        string FieldPattern { get; }

    }
}