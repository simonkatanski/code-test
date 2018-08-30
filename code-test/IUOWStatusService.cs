using System.Threading.Tasks;

namespace code_test
{
    public interface IUOWStatusService
    {
        Task<bool> HasExpiredRetries(string uowId, int maxNumberOfRetries);

        Task<bool> HasExpiredAge(string uowId, long creationEPOCH, int maxAgeInSeconds);

        Task<bool> IsInProcessing(string uowId);

        Task ClearStatusMetadata(string uowId);

        Task ResetInProcessing(string uowId);
    }
}