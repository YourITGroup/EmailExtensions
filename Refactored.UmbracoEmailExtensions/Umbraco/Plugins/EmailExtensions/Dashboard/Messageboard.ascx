<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Messageboard.ascx.cs" Inherits="Refactored.UmbracoEmailExtensions.Umbraco.Plugins.EmailCommunications.Dashboard.Messageboard" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Import Namespace="umbraco.IO" %>
<%@ Import Namespace="System.IO" %>

<umb:CssInclude ID="CssIncludeEmailExtensionsDashboard" runat="server" FilePath="plugins/EmailExtensions/css/MessageBoard.css" PathNameAlias="UmbracoRoot" />
<umb:CssInclude ID="CssIncludeEmailExtensionsGuiDashboard" runat="server" FilePath="plugins/EmailExtensions/css/smoothness.min.css" PathNameAlias="UmbracoRoot" />
<script>
    $(document).ready(function () {
        $("input[type='date']")
                    .datepicker({ dateFormat: 'dd/mm/yy', changeMonth: true, changeYear: true })
                    .get(0)
                    .setAttribute("type", "text");
    })
</script>

<asp:Panel ID="messageBrowserPanel" CssClass="messageBrowser" runat="server">
    <div class="filter">

    <asp:Label ID="filterLabel" runat="server" Text="Filter:" AssociatedControlID="filterTextBox"></asp:Label>
    <asp:TextBox ID="filterTextBox" runat="server"></asp:TextBox>

    <asp:Label ID="startLabel" runat="server" Text="From" 
        AssociatedControlID="startDate"></asp:Label>
    <asp:TextBox ID="startDate" runat="server" type="date" CssClass="date"></asp:TextBox>
    <asp:Label ID="endLabel" runat="server" Text="to" 
        AssociatedControlID="endDate"></asp:Label>
    <asp:TextBox ID="endDate" runat="server" type="date" CssClass="date"></asp:TextBox>

    <asp:Checkbox ID="showArchivedCheckbox" runat="server" Text="Include Archived Messages" AutoPostBack="true"></asp:Checkbox>

    <asp:Button ID="filterButton" runat="server" Text="Show" />

    <asp:Button ID="exportButton" runat="server" Text="Export" OnClick="exportButton_Click" Visible="False" />
    </div>
    <div class="listingWrapper messageContainer">
    <asp:ListView ID="messageListView" runat="server" 
        DataSourceID="messagesObjectDataSource" DataKeyNames="MessageId" OnItemCommand="messageListView_ItemCommand" OnDataBound="messageListView_DataBound">
        <AlternatingItemTemplate>
            <tr class="alternate <%# (bool)Eval("Archived") ? "archived" :  (bool)Eval("Read") ? "read" : "new" %>">
                <td class="actions">
                    <asp:ImageButton ID="readImageButton" runat="server" AlternateText="View Message" ToolTip="View Message" CommandName="Select" CommandArgument='<%# Eval("MessageId") %>' ImageUrl='<%# (bool)Eval("Read") ? "~/Umbraco/Plugins/EmailExtensions/images/email_open.png" : "~/Umbraco/Plugins/EmailExtensions/images/email.png" %>' />
                    <asp:ImageButton ID="archiveImageButton" runat="server" AlternateText='Archive this Message' ToolTip='Archive this Message' CommandName="Archive" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_delete.png" Visible='<%# !(bool)Eval("Archived") %>' ></asp:ImageButton>
                    <asp:ImageButton ID="restoreImageButton" runat="server" AlternateText='Restore this Message' ToolTip='Restore this Message' CommandName="Restore" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_go.png" Visible='<%# (bool)Eval("Archived") %>' ></asp:ImageButton>
                    <asp:ImageButton ID="replyImageButton" runat="server" AlternateText='Reply to this Message' ToolTip='Reply to this Message' CommandName="Reply" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_edit.png" ></asp:ImageButton>
                </td>
                <td class="email">
                    <asp:Label runat="server" Text='<%# Eval("To") %>' ></asp:Label><%-- To --%>
                </td>
                <td class="email">
                    <asp:Label runat="server" Text='<%# Eval("From") %>' ></asp:Label><%-- From --%>
                </td>
                <td class="subject">
                    <asp:Label runat="server" Text='<%# Eval("Subject") %>' ></asp:Label><%-- Subject --%>
                </td>
                <td class="datetime">
                    <asp:Label runat="server" Text='<%# Eval("Sent", "{0:g}") %>' ></asp:Label><%--First Listed--%>
                </td>
<%--                <td class="date<%# (bool)Eval("FollowUpOverdue") ? "Overdue" :  (bool)Eval("FollowUpPending") ? "Pending" : "" %>">
                    <asp:Label runat="server" Text='<%# Eval("FollowupDue", "{0:d}") %>' ></asp:Label>
                </td>--%>
            </tr>
        </AlternatingItemTemplate>
        <ItemTemplate>
            <tr class="<%# (bool)Eval("Archived") ? "archived" :  (bool)Eval("Read") ? "read" : "new" %>">
                <td class="actions">
                    <asp:ImageButton ID="readImageButton" runat="server" AlternateText="View Message" ToolTip="View Message" CommandName="Select" CommandArgument='<%# Eval("MessageId") %>' ImageUrl='<%# (bool)Eval("Read") ? "~/Umbraco/Plugins/EmailExtensions/images/email_open.png" : "~/Umbraco/Plugins/EmailExtensions/images/email.png" %>' />
                    <asp:ImageButton ID="archiveImageButton" runat="server" AlternateText='Archive this Message' ToolTip='Archive this Message' CommandName="Archive" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_delete.png" Visible='<%# !(bool)Eval("Archived") %>' ></asp:ImageButton>
                    <asp:ImageButton ID="restoreImageButton" runat="server" AlternateText='Restore this Message' ToolTip='Restore this Message' CommandName="Restore" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_go.png" Visible='<%# (bool)Eval("Archived") %>' ></asp:ImageButton>
                    <asp:ImageButton ID="replyImageButton" runat="server" AlternateText='Reply to this Message' ToolTip='Reply to this Message' CommandName="Reply" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_edit.png" ></asp:ImageButton>
                </td>
                <td class="email">
                    <asp:Label runat="server" Text='<%# Eval("To") %>' ></asp:Label><%-- To --%>
                </td>
                <td class="email">
                    <asp:Label runat="server" Text='<%# Eval("From") %>' ></asp:Label><%-- From --%>
                </td>
                <td class="subject">
                    <asp:Label runat="server" Text='<%# Eval("Subject") %>' ></asp:Label><%-- Subject --%>
                </td>
                <td class="datetime">
                    <asp:Label runat="server" Text='<%# Eval("Sent", "{0:g}") %>' ></asp:Label><%--First Listed--%>
                </td>
<%--                <td class="date<%# (bool)Eval("FollowUpOverdue") ? "overdue" :  (bool)Eval("FollowUpPending") ? "pending" : "" %>">
                    <asp:Label runat="server" Text='<%# Eval("FollowupDue", "{0:d}") %>' ></asp:Label>
                </td>--%>
            </tr>
        </ItemTemplate>
        <EmptyDataTemplate>
            <table id="Table1" runat="server" style="">
                <tr>
                    <td>No data was returned.</td>
                </tr>
            </table>
        </EmptyDataTemplate>
        <LayoutTemplate>
            <table id="itemPlaceholderContainer" runat="server" border="0" class="listing">
                <tr runat="server">
                    <th runat="server"><%--Commands--%></th>
                    <th runat="server">To</th>
                    <th runat="server">From</th>
                    <th runat="server">Subject</th>
                    <th runat="server">Sent</th>
<%--                    <th runat="server">Followup</th>--%>
                </tr>
                <tr id="itemPlaceholder" runat="server">
                </tr>
            </table>
        </LayoutTemplate>
        <SelectedItemTemplate>
            <tr class="selected <%# (bool)Eval("Archived") ? "archived" :  (bool)Eval("Read") ? "read" : "new" %>">
                <td class="actions">
                    <asp:ImageButton ID="readImageButton" runat="server" AlternateText="View Message" ToolTip="View Message" CommandName="Select" CommandArgument='<%# Eval("MessageId") %>' ImageUrl='<%# (bool)Eval("Read") ? "~/Umbraco/Plugins/EmailExtensions/images/email_open.png" : "~/Umbraco/Plugins/EmailExtensions/images/email.png" %>' />
                    <asp:ImageButton ID="archiveImageButton" runat="server" AlternateText='Archive this Message' ToolTip='Archive this Message' CommandName="Archive" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_delete.png" Visible='<%# !(bool)Eval("Archived") %>' ></asp:ImageButton>
                    <asp:ImageButton ID="restoreImageButton" runat="server" AlternateText='Restore this Message' ToolTip='Restore this Message' CommandName="Restore" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_go.png" Visible='<%# (bool)Eval("Archived") %>' ></asp:ImageButton>
                    <asp:ImageButton ID="replyImageButton" runat="server" AlternateText='Reply to this Message' ToolTip='Reply to this Message' CommandName="Reply" CommandArgument='<%# Eval("MessageId") %>' ImageUrl="~/Umbraco/Plugins/EmailExtensions/images/email_edit.png" ></asp:ImageButton>
                </td>
                <td class="email">
                    <asp:Label runat="server" Text='<%# Eval("To") %>' ></asp:Label><%-- To --%>
                </td>
                <td class="email">
                    <asp:Label runat="server" Text='<%# Eval("From") %>' ></asp:Label><%-- From --%>
                </td>
                <td class="subject">
                    <asp:Label runat="server" Text='<%# Eval("Subject") %>' ></asp:Label><%-- Subject --%>
                </td>
                <td class="datetime">
                    <asp:Label runat="server" Text='<%# Eval("Sent", "{0:g}") %>' ></asp:Label><%--First Listed--%>
                </td>
<%--                <td class="date<%# (bool)Eval("FollowUpOverdue") ? "overdue" :  (bool)Eval("FollowUpPending") ? "pending" : "" %>">
                    <asp:Label runat="server" Text='<%# Eval("FollowupDue", "{0:d}") %>' ></asp:Label>
                </td>--%>
            </tr>
        </SelectedItemTemplate>
    </asp:ListView>
    <asp:ObjectDataSource ID="messagesObjectDataSource" runat="server" 
        SelectMethod="ListMessages" 
        TypeName="Refactored.UmbracoEmailExtensions.Models.MessageManager">
        <SelectParameters>
            <asp:ControlParameter ControlID="filterTextBox" Name="filter" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="startDate" Name="firstDate" PropertyName="Text" Type="DateTime" />
            <asp:ControlParameter ControlID="endDate" Name="lastDate" PropertyName="Text" Type="DateTime" />
            <asp:ControlParameter ControlID="showArchivedCheckbox" Name="showArchived" PropertyName="Checked" Type="Boolean" />
        </SelectParameters>
    </asp:ObjectDataSource>
    </div>
    
    <asp:Panel ID="ConfirmPanel" runat="server" CssClass="confirmBox" Visible="false">
        <asp:Literal runat="server" ID="ConfirmMessage"></asp:Literal>
        <div>
        <asp:Button ID="ConfirmOk" runat="server" Text="Yes" OnCommand="Ok_Command" />
        <asp:Button ID="ConfirmCancel" runat="server" Text="No" />
        </div>
    </asp:Panel>

    <asp:Panel ID="messageView" runat="server" CssClass="messageView messageContainer" visible="false">
        <asp:FormView ID="ReadFormView" runat="server" DataSourceID="odsSelectedMessage">
            <ItemTemplate>
                <fieldset>
                    <legend><%# Eval("Subject") %></legend>
                    <ol>
                        <li><label>Id</label> <span><%# Eval("MessageId") %></span> <label>Date Sent:</label> <span><%# Eval("Sent", "{0:g}") %></span></li>
                        <li>
                            <label>To</label> <span><%# Eval("To") %></span> <%# (bool)Eval("Archived") ? "<label>Archived</label>" : "" %>
                        </li>
                        <li>
                            <label>From</label> <span><%# Eval("From") %></span>
                        </li>
                        <li>
                            <label>CC</label> <span><%# Eval("CC") %></span>
                        </li>
                    </ol>
                    <asp:Panel ID="Panel1" runat="server" Visible='<%# (bool)Eval("RenderHtmlBody") %>' CssClass="messageBody">
                        <%# Eval("SanitisedHtmlBody") %>
                    </asp:Panel>
                    <asp:Panel ID="Panel2" runat="server" Visible='<%# (bool)Eval("RenderPlainBody") %>' CssClass="messageBody">
<pre><%# Eval("PlainBody") %></pre>
                    </asp:Panel>
                </fieldset>
            </ItemTemplate>
        </asp:FormView>
    </asp:Panel>
    <asp:ObjectDataSource ID="odsSelectedMessage" runat="server" 
        SelectMethod="GetMessage" 
        TypeName="Refactored.UmbracoEmailExtensions.Models.MessageManager">
    <SelectParameters>
        <asp:ControlParameter ControlID="messageListView" Name="id" PropertyName="SelectedValue" Type="Int32" />
        <asp:Parameter DefaultValue="true" Name="markAsRead" Type="Boolean" />
    </SelectParameters>
</asp:ObjectDataSource>

    <asp:Panel ID="replyForm" runat="server" CssClass="replyView messageContainer" visible="false">
        <asp:FormView ID="replyFormView" runat="server" DataSourceID="odsReplyMessage" DefaultMode="Edit" OnDataBound="replyFormView_DataBound">
            <EditItemTemplate>
                <asp:HiddenField runat="server" ID="ParentIdHiddenField" Value='<%# Bind("ParentId") %>' />
                <fieldset>
                    <legend>Send Reply</legend>
                    <ol>
                        <li>
                            <asp:Label ID="ToLabel" runat="server" AssociatedControlID="ToTextBox" Text="To"></asp:Label>
                            <asp:TextBox ID="ToTextBox" runat="server" Text='<%# Bind("To") %>' />
                        </li>
                        <li>
                            <asp:Label ID="FromLabel" runat="server" AssociatedControlID="FromTextBox" Text="From"></asp:Label>
                            <asp:TextBox ID="FromTextBox" runat="server" Text='<%# Bind("From") %>' />
                        </li>
                        <li>
                            <asp:Label ID="CCLabel" runat="server" AssociatedControlID="CCTextBox" Text="CC"></asp:Label>
                            <asp:TextBox ID="CCTextBox" runat="server" Text='<%# Bind("CC") %>' />
                        </li>
                        <li>
                            <asp:Label ID="SubjectLabel" runat="server" AssociatedControlID="SubjectTextBox" Text="Subject"></asp:Label>
                            <asp:TextBox ID="SubjectTextBox" runat="server" Text='<%# Bind("Subject") %>' />
                        </li>
                        <li>
                            <asp:PlaceHolder runat="server" ID="RichTextPlaceholder"></asp:PlaceHolder>
<%--                <asp:TextBox ID="HtmlBodyTextBox" runat="server" Text='<%# Bind("HtmlBody") %>' TextMode="MultiLine" Visible='<%# Eval("RenderHtmlBody") %>' />--%>
<%--                <asp:TextBox ID="PlainBodyTextBox" runat="server" Text='<%# Bind("PlainBody") %>' TextMode="MultiLine" Visible='<%# Eval("RenderPlainBody") %>' />--%>

                        </li>
                        <li class="buttons">
                            <asp:Button ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" Text="Send" />
                            &nbsp;<asp:LinkButton ID="UpdateCancelButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />
                        </li>
                    </ol>
                </fieldset>
            </EditItemTemplate>
        </asp:FormView>
        <asp:ObjectDataSource ID="odsReplyMessage" runat="server" SelectMethod="CreateReply" TypeName="Refactored.UmbracoEmailExtensions.Models.MessageManager" 
            DataObjectTypeName="Refactored.UmbracoEmailExtensions.Models.Message" UpdateMethod="Update" OnUpdating="odsReplyMessage_Updating">
            <SelectParameters>
                <asp:ControlParameter ControlID="messageListView" Name="messageId" PropertyName="SelectedValue" Type="Int32" />
                <asp:Parameter Name="quoteMessage" Type="Boolean" DefaultValue="true" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </asp:Panel>
</asp:Panel>