using System.Threading.Tasks;

namespace code_test
{
    public interface IUOWStatusService
    {
        /// <summary>
        /// Checks if the RingbaUOW by the given uowId exceeded the number of retries.
        /// </summary>
        /// <param name="uowId">Id of the given RingbaUOW</param>
        /// <param name="maxNumberOfRetries">Max number of retries to validate against</param>
        /// <returns>True if RingbaUOW has been reached the retry limit</returns>
        Task<bool> HasExpiredRetries(string uowId, int maxNumberOfRetries);

        /// <summary>
        /// Checks if RingbaUOW by the given uowId has exceeded the maximum age
        /// </summary>
        /// <param name="uowId">Id of the given RingbaUOW</param>
        /// <param name="creationEPOCH">The epoch this Unit of Work was created</param>
        /// <param name="maxAgeInSeconds">The max age in seconds this Unit of Work is valid for</param>
        /// <returns></returns>
        Task<bool> HasExpiredAge(string uowId, long creationEPOCH, int maxAgeInSeconds);

        /// <summary>
        /// Tries to set the InProcessing flag for the RingbaUOW by the given uowId to True.
        /// </summary>
        /// <param name="uowId"></param>
        /// <returns>True if this RingbaUOW is not currently being processed. False if it is being processed.</returns>
        Task<bool> TrySetInProcessing(string uowId);

        /// <summary>
        /// Removes the metadata for in processing and expiry validations.
        /// </summary>        
        Task ClearStatusMetadata(string uowId);

        /// <summary>
        /// Resets the InProcessing flag to false.
        /// </summary>
        Task ResetInProcessing(string uowId);
    }
}