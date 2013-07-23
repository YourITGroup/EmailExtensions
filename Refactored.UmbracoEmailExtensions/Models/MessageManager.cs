using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Refactored.UmbracoEmailExtensions.Persistence;
using Umbraco.Web;

namespace Refactored.UmbracoEmailExtensions.Models
{
    public static class MessageManager
    {
        private static bool initialised = false;
        internal static Database Database
        {
            get
            {
                if (!initialised)
                {
                    // Make sure the database is initialised properly.
                    ApplicationContext.Current.DatabaseContext.Database.InitialiseEmailDatabase();
                    initialised = true;
                }
                return ApplicationContext.Current.DatabaseContext.Database;
            }
        }

        /// <summary>
        /// Creates a new Email Message
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="htmlBody"></param>
        /// <param name="textBody"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static Message CreateMessage(string from, string to, string subject, string htmlBody, string textBody, string cc = null)
        {
            var message = new Message {To = to, From = from, Subject = subject, HtmlBody = htmlBody, PlainBody = textBody, CC = cc, Sent = DateTime.Now };
            Update(message);
            return message;
        }

        /// <summary>
        /// Initialises a Reply message but doesn't save it in the database.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="quoteMessage"></param>
        /// <returns></returns>
        public static Message CreateReply(int messageId, bool quoteMessage = true)
        {
            var parent = GetMessage(messageId, true);
            if (parent == null)
                return null;

            string subject = parent.Subject.StartsWith("re:", StringComparison.OrdinalIgnoreCase) ? parent.Subject : "Re: " + parent.Subject;
            string htmlMessage = quoteMessage && parent.RenderHtmlBody ? string.Format("<p> </p><p>At {0:g} {1} wrote:</p><hr>{2}", parent.Sent, parent.From, parent.SanitisedHtmlBody) : string.Empty;
            string plainMessage = quoteMessage && !string.IsNullOrWhiteSpace(parent.PlainBody) ? string.Format(@"

At {0:g} {1} wrote:
---------------------------------
{1}", parent.Sent, parent.From, parent.PlainBody) : string.Empty;

            return new Message { ParentId = parent.MessageId, To = parent.From, From = parent.To, Subject = subject, HtmlBody = htmlMessage, PlainBody = plainMessage };

        }

        /// <summary>
        /// Marks a message as Read
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Message MarkAsRead(this Message message)
        {
            if (message.Read)
                return message;

            message.Read = true;
            Update(message);
            return message;
        }

        /// <summary>
        ///  Marks a message as Spam
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Message MarkAsSpam(this Message message)
        {
            if (message.Spam)
                return message;

            message.Spam = true;
            Update(message);
            return message;
        }

        /// <summary>
        /// Marks a message as Not Spam
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Message MarkAsHam(this Message message)
        {
            if (!message.Spam)
                return message;

            message.Spam = false;
            Update(message);
            return message;
        }

        /// <summary>
        /// Archives a Message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Message Archive(this Message message)
        {
            if (message.Archived)
                return message;

            message.Archived = true;
            message.ArchivedDate = DateTime.Now;
            message.ArchivedUserId = UmbracoContext.Current.UmbracoUser.Id;

            Update(message);
            return message;
        }

        /// <summary>
        /// Restores a Message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Message UnArchive(this Message message)
        {
            if (!message.Archived)
                return message;

            message.Archived = false;
            message.ArchivedDate = null;
            message.ArchivedUserId = null;

            Update(message);
            return message;
        }

        /// <summary>
        /// Updates the specified message to the database after wrapping the operation in a Transaction
        /// </summary>
        /// <param name="message"></param>
        public static void Update(Message message)
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.Save(message);
                transaction.Complete();
            }
        }

        public static Message GetMessage(int id, bool markAsRead = false)
        {
            var message = Database.SingleOrDefault<Message>(id);
            if (message != null && markAsRead)
                message = message.MarkAsRead();
            return message;
        }

        public static IEnumerable<Message> ListMessages(string filter = "", DateTime? firstDate = null, DateTime? lastDate = null, bool showArchived = false)
        {
            var sql=Sql.Builder;

            bool hasWhere = false;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                sql = sql.Append("WHERE (to like '%@0%'", filter)
                        .Append("OR from like '%@0%'", filter)
                        .Append("OR subject like '%@0%'", filter)
                        .Append("OR htmlBody like '%@0%'", filter)
                        .Append("OR plain like '%@0%')", filter);
                hasWhere = true;
            }

            if (!showArchived)
            {
                if (!hasWhere)
                {
                    sql = sql.Append("WHERE ");
                }
                else
                {
                    sql = sql.Append(" AND ");
                }
                sql = sql.Append("(archived = 0)");
                hasWhere = true;
            }

            if (firstDate.HasValue)
            {
                if (!hasWhere)
                {
                    sql = sql.Append("WHERE ");
                }
                else
                {
                    sql = sql.Append(" AND ");
                }
                sql = sql.Append("(sentDate >= @0)", firstDate.Value.Date);
                hasWhere = true;
            }

            if (lastDate.HasValue)
            {
                if (!hasWhere)
                {
                    sql = sql.Append("WHERE ");
                }
                else
                {
                    sql = sql.Append(" AND ");
                }
                sql = sql.Append("(sentDate <= @0)", lastDate.Value.Date.AddDays(1).AddMinutes(-1));
                hasWhere = true;
            }
            return Database.Query<Message>(sql.OrderBy("sentDate DESC"));
        }

        public static IEnumerable<Message> ListMessagesForEmail(string emailAddress, string filter = "", DateTime? firstDate = null, DateTime? lastDate = null, bool showArchived = false)
        {
            var sql = Sql.Builder.Append("WHERE (to = @0)", emailAddress);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                sql = sql.Append("AND (from like '%@0%'", filter)
                        .Append("OR subject like '%@0%'", filter)
                        .Append("OR htmlBody like '%@0%'", filter)
                        .Append("OR plain like '%@0%')", filter);
            }

            if (!showArchived)
            {
                sql = sql.Append(" AND (archived = 0)");
            }

            if (firstDate.HasValue)
            {
                sql = sql.Append(" AND (sentDate >= @0)", firstDate.Value.Date);
            }

            if (lastDate.HasValue)
            {
                sql = sql.Append(" AND (sentDate <= @0)", lastDate.Value.Date.AddDays(1).AddMinutes(-1));
            }
            return Database.Query<Message>(sql.OrderBy("sentDate DESC"));
        }
    }
}