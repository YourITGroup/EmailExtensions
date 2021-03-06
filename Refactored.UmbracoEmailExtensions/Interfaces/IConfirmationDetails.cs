﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Refactored.UmbracoEmailExtensions.Interfaces
{
    public interface IConfirmationDetails
    {
        string FromEmail { get; }
        string FieldPattern { get; }
        string SubmitterEmail { get; }
        int HtmlConfirmationTemplateId { get; }
        int TextConfirmationTemplateId { get; }
    }
}
