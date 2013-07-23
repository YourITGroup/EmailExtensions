using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Refactored.UmbracoEmailExtensions.Models
{
    [TableName("mcEmailMessage")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public partial class Message
    {
        //[Column("id")]
        //[Constraint(Default = "newid()")]
        //public Guid MessageId { get; set; }

        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_emailMessage", IdentitySeed = 0)]
        public int MessageId { get; set; }

        [Column("parentID")]
        [ForeignKey(typeof(Message))]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_emailMessageParentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? ParentId { get; set; }
        
        //[Column("category")]
        //[Length(100)]
        //[NullSetting(NullSetting = NullSettings.Null)]
        //public string Category { get; set; }

        [Column("to")]
        [Length(250)]
        public string To { get; set; }

        [Column("from")]
        [Length(250)]
        public string From { get; set; }

        [Column("cc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CC { get; set; }

        [Column("sentDate")]
        [Constraint(Default = "getdate()")]
        public DateTime Sent { get; set; }

        [Column("subject")]
        [Length(250)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Subject { get; set; }

        [Column("htmlBody")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string HtmlBody { get; set; }

        [Column("plainBody")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string PlainBody { get; set; }

        [Column("flagged")]
        [Constraint(Default = "0")]
        public bool Flagged { get; set; }

        [Column("followupDue")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? FollowupDue { get; set; }
        
        [Column("followedUp")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? FollowedUp { get; set; }

        [Column("archived")]
        [Constraint(Default = "0")]
        public bool Archived { get; set; }

        [Column("archivedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? ArchivedDate { get; set; }

        [Column("archivedUserId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? ArchivedUserId { get; set; }

        [Column("spam")]
        [Constraint(Default = "0")]
        public bool Spam { get; set; }

        [Column("read")]
        [Constraint(Default = "0")]
        public bool Read { get; set; }
    }
}