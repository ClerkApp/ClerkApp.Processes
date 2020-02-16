using System;
using Clerk.Processes.Common.Logger;
using Clerk.Processes.DbCleanup.Infrastructure;
using Serilog;

namespace Clerk.Processes.DbCleanup
{
    internal class Program
    {
        private static void Main()
        {
            Log.Logger = DocumentStoreLogger.LoggerConfig;

            try
            {
                DocumentStoreHelper.Cleanup();
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
