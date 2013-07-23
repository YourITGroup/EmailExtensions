using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Refactored.UmbracoEmailExtensions.Config;
using Refactored.UmbracoEmailExtensions.Models;
using umbraco.cms.businesslogic.datatype;
using umbraco.editorControls.tinyMCE3;
using umbraco.uicontrols;
using Umbraco.Web;

namespace Refactored.UmbracoEmailExtensions.Umbraco.Plugins.EmailCommunications.Dashboard
{
    public partial class Messageboard : UmbracoUserControl
    {
        private TinyMCE tinyMce;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var date = DateTime.Now;
                endDate.Text = date.ToShortDateString();
                startDate.Text = date.AddDays(-14).ToShortDateString();
            }
        }

        protected void exportButton_Click(object sender, EventArgs e)
        {

        }

        protected void messageListView_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            replyForm.Visible = false;

            int id = 0;
            if (int.TryParse(e.CommandArgument.ToString(), out id))
            {
                var message = MessageManager.GetMessage(id);

                switch (e.CommandName.ToLower())
                {
                    case "select":
                        message.MarkAsRead();
                        messageView.Visible = true;
                        break;
                    case "archive":
                        message.Archive();
                        break;
                    case "restore":
                        message.UnArchive();
                        break;
                    case "reply":
                        // Show the Reply form.
                        messageListView.SelectItem(e.Item.DataItemIndex);
                        messageView.Visible = false;
                        replyForm.Visible = true;
                        break;
                }
            }
            messageListView.DataBind();

            ConfirmPanel.Visible = false;
        }

        protected void Ok_Command(object sender, CommandEventArgs e)
        {
            messageListView.DataBind();
            ConfirmPanel.Visible = false;
        }

        private void ShowConfirmPanel(string message, string command = "", object argument = null, bool showCancel = true)
        {
            ConfirmCancel.Visible = !string.IsNullOrWhiteSpace(command) && showCancel;
            ConfirmMessage.Text = message;
            ConfirmOk.CommandName = command;
            ConfirmOk.CommandArgument = argument == null ? "" : argument.ToString();
            ConfirmOk.Text = !string.IsNullOrWhiteSpace(command) ? "Yes" : "Ok";
            ConfirmPanel.Visible = true;
        }

        protected void odsReplyMessage_Updating(object sender, ObjectDataSourceMethodEventArgs e)
        {
            // Need to retrieve the value of the richtext field.
            PlaceHolder RichTextPlaceholder = replyFormView.FindControl("RichTextPlaceholder") as PlaceHolder;
            if (RichTextPlaceholder != null)
            {
                RichTextPlaceholder.Controls.Add(tinyMce);
                tinyMce.Text = Request.Form[tinyMce.UniqueID];
            }

            // Send out the new email.
            // TODO: Provide a template to render the content in.
            var message = e.InputParameters[0] as Message;
            if (message == null)
                return;

            // Get the Html back from the TinyMce control.
            message.HtmlBody = Request.Form[tinyMce.UniqueID];
            // Set the plainBody to a version of the stripped content.
            message.PlainBody = Umbraco.StripHtml(message.HtmlBody).ToString();
            
            message.Sent = DateTime.Now;

            try
            {
                Refactored.Email.Email.SendEmail(message.From, message.To, message.Subject, message.HtmlBody, message.PlainBody, cc: message.CC);
                replyForm.Visible = false;
                ShowConfirmPanel(string.Format("Message \"{0}\" has been sent to {1}", message.Subject, message.To));
            }
            catch (Exception ex)
            {
                ShowConfirmPanel(ex.Message);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var parent = this.Parent;
            while (parent != null && !(parent is TabPage))
            {
                parent = parent.Parent;
            }
            if (parent != null)
            {
                ((TabPage)parent).Menu.NewElement("div", "tinyMCEMenu1", "tinymceMenuBar", 0);
                DataTypeDefinition dataTypeDefinition1 = DataTypeDefinition.GetDataTypeDefinition(Configuration.TinyMceDataTypeId);
                tinyMce = (umbraco.editorControls.tinyMCE3.TinyMCE)dataTypeDefinition1.DataType.DataEditor;
                tinyMce.config.Add("umbraco_toolbar_id", "tinyMCEMenu1");
                tinyMce.ID = "HtmlBodyTextBox";

            }
        }

        protected void replyFormView_DataBound(object sender, EventArgs e)
        {
            PlaceHolder RichTextPlaceholder = replyFormView.FindControl("RichTextPlaceholder") as PlaceHolder;
            if (RichTextPlaceholder == null)
                return;

            var message = replyFormView.DataItem as Message;

            if (message != null && tinyMce != null)
            {
                tinyMce.Text = message.SanitisedHtmlBody;
                // Get the HtmlBody value and create a Data object for the tinyMce control.
                RichTextPlaceholder.Controls.Add(tinyMce);
            }
        }

        protected void messageListView_DataBound(object sender, EventArgs e)
        {
            if (ReadFormView.Visible)
                ReadFormView.DataBind();

            if (replyFormView.Visible)
                replyFormView.DataBind();
        }

    }
}