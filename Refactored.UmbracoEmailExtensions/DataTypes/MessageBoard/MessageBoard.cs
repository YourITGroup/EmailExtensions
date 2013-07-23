using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.interfaces;
using umbraco.uicontrols;
using Umbraco.Core.IO;

namespace Refactored.UmbracoEmailExtensions.DataTypes.MessageBoard
{
    public class MessageBoard : WebControl, IDataEditor
    {
        public Control Editor { get { return this; } }

        public virtual bool TreatAsRichTextEditor { get { return false; } }
        public bool ShowLabel { get { return false; } }

        public void Save()
        {
        }

        private readonly string _usercontrolPath = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/plugins/EmailExtensions/Dashboard/MessageBoard.ascx";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            base.Controls.Add(new UserControl().LoadControl(_usercontrolPath));
        }
        protected override void Render(HtmlTextWriter writer)
        {
            var parent = this.Parent;
            while (parent != null && !(parent is TabPage))
            {
                parent = parent.Parent;
            }
            if (parent != null)
            {
                // Write some style information for the tab container.
                string styles = "<style>#" + parent.ClientID + " .tabpageContent, #" + parent.ClientID + " .tabpageContent>div, #" + parent.ClientID + " .propertypane {position: relative; height:99%} .messageBrowser .filter { top: 0; }</style>";
                writer.WriteLine(styles);
            }
            base.Render(writer);
        }
    }
}