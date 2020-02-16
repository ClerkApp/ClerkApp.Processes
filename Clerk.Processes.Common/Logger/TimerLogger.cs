using System;
using System.Diagnostics;
using System.Globalization;
using System.Timers;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Clerk.Processes.Common.Logger
{
    public class TimerLogger : IDisposable
    {
        private static int propertyValue;
        private static string timedMessage;
        private static LogEventLevel logEventLevel;

        private readonly Timer seedTimer;
        private readonly Stopwatch totalDuration;

        private readonly string operationName;

        public TimerLogger(string operationName = null, double intervalTimeInSeconds = 5)
        {
            this.operationName = operationName;
            totalDuration = new Stopwatch();

            totalDuration = new Stopwatch();
            seedTimer = new Timer
                            {
                                Interval = intervalTimeInSeconds * 1000,
                                AutoReset = true,
                                Enabled = true
                            };
            seedTimer.Elapsed += OnTimedEvent;
        }

        public void UpdatePropertyValue(int value) => propertyValue = value;

        [MessageTemplateFormatMethod("messageTemplate")]
        public void OnTimedEventTemplate(string messageTemplate, int property, LogEventLevel logLevel = LogEventLevel.Information)
        {
            propertyValue = property;
            timedMessage = messageTemplate;
            logEventLevel = logLevel;
        }

        public void StartTimer()
        {
            totalDuration.Start();
            seedTimer.Start();
        }

        public void Dispose()
        {
            totalDuration.Stop();
            seedTimer.Stop();

            var elapsedTime = totalDuration.Elapsed.ToString("mm\\:ss");

            if (string.IsNullOrWhiteSpace(operationName))
            {
                Log.Information("Procces total time took: {0}", elapsedTime);
            }
            else
            {
                Log.Information("Procces for {0} total time took: {1}", operationName, elapsedTime);
            }
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(timedMessage))
            {
                Log.Information("Still working, please wait...");
            }
            else
            {
                Log.Write(logEventLevel, timedMessage, propertyValue.ToString("N0", CultureInfo.InvariantCulture));
            }
        }
    }
}
