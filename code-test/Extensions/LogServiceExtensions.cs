using RingbaLibs;
using System;
using System.Threading.Tasks;

namespace code_test.Extensions
{
    public static class LogServiceExtensions
    {
        public static async Task LogInfoAsync(this ILogService logService, string logId, string logMessage, params object[] args)
        {
            await logService.LogAsync(logId, logMessage, LOG_LEVEL.INFO, args);
        }

        public static void LogInfo(this ILogService logService, string logId, string logMessage, params object[] args)
        {
            Task.Run(() => logService.LogAsync(logId, logMessage, LOG_LEVEL.INFO, args)).Wait();            
        }

        public static async Task LogExceptionAsync(this ILogService logService, string logId, string logMessage, params object[] args)
        {
            await logService.LogAsync(logId, logMessage, LOG_LEVEL.EXCEPTION, args);
        }

        public static async Task LogExceptionAsync(this ILogService logService, string logId, Exception ex, string logMessage, params object[] args)
        {
            string fullExceptionInfo = ex.ToString();            
            await logService.LogAsync(logId, $"{logMessage}{Environment.NewLine}{fullExceptionInfo}", LOG_LEVEL.EXCEPTION, args);
        }

        public static async Task LogCriticalAsync(this ILogService logService, string logId, string logMessage, params object[] args)
        {
            await logService.LogAsync(logId, logMessage, LOG_LEVEL.CRITICAL, args);
        }

        public static async Task LogWarningAsync(this ILogService logService, string logId, string logMessage, params object[] args)
        {
            await logService.LogAsync(logId, logMessage, LOG_LEVEL.WARNING, args);
        }
    }
}