using System;
using System.Collections.Generic;
using Clerk.Processes.Common;
using Clerk.Processes.Common.Logger;
using Clerk.Processes.Common.RavenDb;
using Clerk.Processes.DbMigrations.Core.Entities.Product.Phones;
using Clerk.Processes.DbMigrations.Core.Entities.Store;
using Faker;
using FizzWare.NBuilder;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Serilog;

namespace Clerk.Processes.DbMigrations.Infrastructure
{
    public class ClerkFakeInitializer
    {
        private static int seedCounter;

        private static SessionOptions options;

        public static void Initialize()
        {
            foreach (var configurationItem in Configuration.Init())
            {
                DocumentStoreHolder.Init(configurationItem.Endpoint);

                foreach (var destination in configurationItem.ToPopulate)
                {
                    options = new SessionOptions { Database = destination.Database };
                    SeedEverything();
                }
            }
        }

        public static void SeedEverything()
        {
            CreateDatabaseIfNotExists();

            PopulateDatabase();
        }

        private static void PopulateDatabase()
        {
            SeedProductPhones(50000);
        }

        private static void CreateDatabaseIfNotExists()
        {
            try
            {
                if (DatabaseNotExists(DocumentStoreHolder.Store, options.Database))
                {
                    DocumentStoreHolder.Store.Maintenance.Server.Send(
                        new CreateDatabaseOperation(new DatabaseRecord(options.Database)));
                }
                else
                {
                    Log.Information("RavenDB database exist seeding with initial data");
                }
            }
            catch (Exception exception)
            {
                Log.Error("Exception when deleting database: {0}.", options.Database);
                Log.Debug(exception.Message);
            }
        }

        private static bool DatabaseNotExists(IDocumentStore store, string database = null)
        {
            database = database ?? store.Database;

            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(database));

            try
            {
                store.Maintenance.ForDatabase(database).Send(new GetStatisticsOperation());
            }
            catch (DatabaseDoesNotExistException)
            {
                return true;
            }

            return false;
        }

        private static void SeedProductPhones(int seedNumber)
        {
            try
            {
                using (var timerLogger = new TimerLogger("Seeding db with phones"))
                using (var bulkInsert = DocumentStoreHolder.Store.BulkInsert(options.Database))
                {
                    Log.Information("Started building the faker database objects");

                    var platform = Builder<Platform>.CreateListOfSize(200).All()
                        .With(p => p.Chipset = Builder<Chipset>.CreateNew().Build())
                        .With(p => p.Cpu = Builder<Cpu>.CreateNew().Build()).With(
                            p => p.Os = Builder<Os>.CreateNew()
                                     .With(o => o.NameList = new List<string>(Lorem.Words(5))).Build())
                        .Random(100).Build();

                    var memory = Builder<Memory>.CreateListOfSize(30).All()
                        .With(m => m.Internal = Builder<InternalMemory>.CreateNew().Build())
                        .With(m => m.External = Builder<ExternalMemory>.CreateNew().Build()).Random(15).Build();

                    var frontCamera = Builder<Camera>.CreateListOfSize(45).All()
                        .With(f => f.Specs = new List<string>(Lorem.Paragraphs(3)))
                        .With(f => f.Video = new List<string>(Lorem.Words(2)))
                        .With(f => f.Features = new List<string>(Lorem.Sentences(4))).Random(30).Build();

                    var backCamera = Builder<Camera>.CreateListOfSize(45).All()
                        .With(f => f.Specs = new List<string>(Lorem.Paragraphs(2)))
                        .With(f => f.Video = new List<string>(Lorem.Words(3)))
                        .With(f => f.Features = new List<string>(Lorem.Sentences(6))).Random(25).Build();

                    var sound = Builder<Sound>.CreateListOfSize(10).All()
                        .With(s => s.Feature = new List<string>(Lorem.Sentences(2))).Build();

                    var battery = Builder<Battery>.CreateListOfSize(25).All()
                        .With(s => s.Feature = new List<string>(Lorem.Sentences(3))).Build();

                    var feature = Builder<Feature>.CreateListOfSize(30).All()
                        .With(s => s.Sensors = new List<string>(Lorem.Words(5))).Random(30).Build();

                    var store = Builder<StoreProduct>.CreateListOfSize(50).All().Random(30).Build();

                    var mobilePhones = Builder<MobilePhone>.CreateListOfSize(seedNumber).All()
                        .With(p => p.Platform = Pick<Platform>.RandomItemFrom(platform))
                        .With(p => p.Memory = Pick<Memory>.RandomItemFrom(memory))
                        .With(p => p.FrontCamera = Pick<Camera>.RandomItemFrom(frontCamera))
                        .With(p => p.BackCamera = Pick<Camera>.RandomItemFrom(backCamera))
                        .With(p => p.Sound = Pick<Sound>.RandomItemFrom(sound))
                        .With(p => p.Battery = Pick<Battery>.RandomItemFrom(battery))
                        .With(p => p.Features = Pick<Feature>.RandomItemFrom(feature))
                        .With(p => p.Comms = Builder<Comms>.CreateNew().Build())
                        .With(p => p.Id = Guid.NewGuid().ToString()).With(p => p.CategoryId = Guid.NewGuid().ToString())
                        .With(
                            p => p.ProductName = new List<string>
                            {
                                                         $"Product-{Identification.UKNationalInsuranceNumber()}"
                                                     }).With(p => p.Store = Pick<StoreProduct>.RandomItemFrom(store))
                        .Build();

                    timerLogger.OnTimedEventTemplate("Processing: {0} items so far...", seedCounter);
                    timerLogger.StartTimer();

                    Log.Information("Ready to start seeded database with {0:n0} phones", seedNumber);

                    foreach (var mobilePhone in mobilePhones)
                    {
                        bulkInsert.Store(mobilePhone);
                        timerLogger.UpdatePropertyValue(seedCounter++);
                    }
                }
            }
            catch (Exception unknownException)
            {
                Log.Error(
                    unknownException,
                    "Failed while trying to seed the {Database} database with {SeedNumber} phones.", 
                    options.Database,
                    seedNumber);
            }
            finally
            {
                Log.Information("Seeded database with {0:n0} phones", seedCounter);
                seedCounter = 0;
            }
        }
    }
}
