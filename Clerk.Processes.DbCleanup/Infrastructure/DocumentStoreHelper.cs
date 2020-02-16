using System;
using System.Reflection.Metadata;
using Clerk.Processes.Common;
using Clerk.Processes.Common.RavenDb;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Serilog;

namespace Clerk.Processes.DbCleanup.Infrastructure
{
    public class DocumentStoreHelper
    {
        public static void Cleanup()
        {
            foreach (var configurationItem in Configuration.Init())
            {
                DocumentStoreHolder.Init(configurationItem.Endpoint);

                foreach (var itemToDelete in configurationItem.ToDelete)
                {
                    var options = new SessionOptions { Database = itemToDelete.Database };

                    if (itemToDelete.Collection.Equals("ALL"))
                    {
                        ClearDatabase(options);
                        continue;
                    }

                    DeleteItemsFromCollection(options, itemToDelete.Collection);
                }
            }
        }

        public static void ClearDatabase(SessionOptions options)
        {
            try
            {
                using (var session = DocumentStoreHolder.Store.OpenSession(options))
                {
                    var parameters = new DeleteDatabasesOperation.Parameters
                    {
                        DatabaseNames = new[] { options.Database },
                        HardDelete = true
                    };

                    DocumentStoreHolder.Store.Maintenance.Server.Send(new DeleteDatabasesOperation(parameters));
                    DocumentStoreHolder.Store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(options.Database)));

                    session.SaveChanges();
                }

                Log.Information("Succesfully cleaned database: {0}.", options.Database);
            }
            catch (Exception exception)
            {
                Log.Error("Exception when deleting database: {0}.", options.Database);
                Log.Debug(exception.Message);
            }
        }

        public static void DeleteItemsFromCollection(
            SessionOptions options,
            string collection)
        {
            try
            {
                var count = 0;

                using (var session = DocumentStoreHolder.Store.OpenSession(options))
                {
                    var query = session.Advanced.DocumentQuery<object>(collectionName: collection);

                    var results = session.Advanced.Stream(query, out _);

                    while (results.MoveNext())
                    {
                        var employee = results.Current;
                        if (employee == null) continue;
                        session.Delete(employee.Id);
                        count++;
                    }

                    session.SaveChanges();
                }

                Log.Information("Deleting {0} items from db : {1} - col : {2}.", count, options.Database, collection);
            }
            catch (Exception e)
            {
                Log.Error("Exception when deleting items from db : {0} - col : {1} : {2}", options.Database, collection, e.Message);
            }
        }
    }
}
