using System;
using System.Threading.Tasks;

namespace RingbaLibs
{
    public interface ILogService : IDisposable
    {
        Task LogAsync(string logId, string logMessage, LOG_LEVEL level, params object[] args);

    }

    public enum LOG_LEVEL
    {
        INFO,
        WARNING,
        EXCEPTION,
        CRITICAL
    }
}