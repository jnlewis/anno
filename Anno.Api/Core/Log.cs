using NLog;
using System;

namespace Anno.Api.Core
{
    public static class Log
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Trace(string message) { logger.Trace(message); }

        public static void Debug(string message) { logger.Debug(message); }

        public static void Info(string message) { logger.Info(message); }
        public static void Info(string message, params object[] args) { logger.Info(message, args); }

        public static void Warn(string message) { logger.Warn(message); }

        public static void Error(string message) { logger.Error(message); }

        public static void Fatal(string message) { logger.Fatal(message); }

        public static void Exception(Exception ex)
        {
            Error(ex.Message);
            //TODO: write inner exceptions and stacktrace
        }
    }
}