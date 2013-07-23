using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Refactored.UmbracoEmailExtensions.Interfaces
{
    public interface ISubmitDetails
    {
        string ToEmail { get;  }
        string FromEmail { get; }
        string BccEmail { get; }
        int HtmlTemplateId { get; }
        int TextTemplateId { get; }
        string FieldPattern { get; }
    }
}