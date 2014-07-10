using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Refactored.UmbracoEmailExtensions.Interfaces
{
    public interface ISubmitDetails
    {
        string FromEmail { get; }
        string FieldPattern { get; }
        string ToEmail { get;  }
        string BccEmail { get; }
        int HtmlTemplateId { get; }
        int TextTemplateId { get; }
    }
}