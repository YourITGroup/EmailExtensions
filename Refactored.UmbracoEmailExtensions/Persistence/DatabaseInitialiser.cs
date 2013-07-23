using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Refactored.UmbracoEmailExtensions.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Refactored.UmbracoEmailExtensions.Persistence
{
    internal static class DatabaseInitialiser
    {
        internal static void InitialiseEmailDatabase(this Database database) {
            //if (database.TableExist("mcEmailMessage"))
            //    return;
            // Create table automatically wraps the operation in a Transaction
            database.CreateTable<Message>();
        }
    }
}