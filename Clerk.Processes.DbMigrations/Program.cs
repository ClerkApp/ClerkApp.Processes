using System;
using Clerk.Processes.Common.Logger;
using Clerk.Processes.DbMigrations.Infrastructure;
using Serilog;

namespace Clerk.Processes.DbMigrations
{
    public class Program
    {
        public static void Main()
        {
            Log.Logger = DocumentStoreLogger.LoggerConfig;

            try
            {
                ClerkFakeInitializer.Initialize();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
