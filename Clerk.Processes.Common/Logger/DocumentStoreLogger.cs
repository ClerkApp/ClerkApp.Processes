using System;
using System.IO;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace Clerk.Processes.Common.Logger
{
    public static class DocumentStoreLogger
    {
        private static readonly string FileJsonPath = GetFilePath(fileName: "json", folderLevel: 3);
        private static readonly string FilePath = GetFilePath(folderLevel: 3);

        private static readonly Lazy<ILogger> LazyLogger = new Lazy<ILogger>(CreateLogger);

        public static ILogger LoggerConfig => LazyLogger.Value;

        private static ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(
                    FilePath,
                    rollingInterval: RollingInterval.Day)
                .WriteTo.File(
                    new JsonFormatter(renderMessage: true),
                    FileJsonPath,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        private static string GetFilePath(string fileName = "log", string folderName = "logs", int folderLevel = 0)
        {
            var rootPath = GetPathAtLevel(folderLevel);

            return $@"{rootPath}\{folderName}\{fileName}-.txt";
        }

        private static string GetPathAtLevel(int folderLevel = 0)
        {
            var fullPath = Directory.GetCurrentDirectory();
            while (folderLevel-- > 0)
            {
                fullPath = Path.GetDirectoryName(fullPath);
            }

            return fullPath;
        }
    }
}
