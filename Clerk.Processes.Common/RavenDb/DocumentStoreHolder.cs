using System;
using Raven.Client.Documents;
using Serilog;

namespace Clerk.Processes.Common.RavenDb
{
    // The `DocumentStoreHolder` class holds a single Document Store instance.
    public static class DocumentStoreHolder
    {
        // Use Lazy<IDocumentStore> to initialize the document store lazily. 
        // This ensures that it is created only once - when first accessing the public `Store` property.
        private static readonly Lazy<IDocumentStore> LazyStore = new Lazy<IDocumentStore>(CreateStore);

        public static IDocumentStore Store => LazyStore.Value;

        public static string Endpoint { get; private set; }

        public static void Init(string endpoint)
        {
            Endpoint = endpoint;
        }

        private static IDocumentStore CreateStore()
        {
            IDocumentStore documentStore;
            try
            {
                documentStore = new DocumentStore
                {
                    // Define the cluster node URLs (required)
                    Urls = new[] { Endpoint },

                    // Set conventions as necessary (optional)
                    Conventions =
                    {
                        MaxNumberOfRequestsPerSession = 10,
                        UseOptimisticConcurrency = true
                    }

                    // Define a default database (optional)
                    // Database = "your_database_name",

                    // Define a client certificate (optional)
                    // Certificate = new X509Certificate2("C:\\path_to_your_pfx_file\\cert.pfx"),
                }.Initialize();

                Log.Information("Initialized RavenDB document store at {0}", Endpoint);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException)
                {
                    Log.Error("You need to initialize the Endpoint properties usint Init before creating the document store.");
                }
                else
                {
                    Log.Error(ex, "Failed while trying to initialize the RavenDB store for {Endpoint}.", Endpoint);
                }

                Environment.Exit(6);
                throw;
            }

            return documentStore;
        }
    }
}
