using code_test.ExisitingInterfaces.Models;
using code_test.Extensions;
using RingbaLibs;
using RingbaLibs.Models;
using System;
using System.Threading.Tasks;

namespace code_test
{
    public class UOWStatusService : IUOWStatusService
    {
        private readonly IKVRepository _repository;
        private readonly ILogService _logService;
        private readonly string _logId;

        public UOWStatusService(
            IKVRepository repository,
            ILogService logService)
        {
            _repository = repository;
            _logService = logService;
            _logId = typeof(UOWStatusService).Name;
        }

        public async Task ResetInProcessing(string uowId)
        {
            var result = await _repository.GetAsync<UowStatus>(uowId);
            if (!result.IsSuccessfull)
                throw new InvalidOperationException($"Retrieval of uow status information for {uowId} failed with message: {result.ErrorMessage} and code: {result.ErrorCode}");

            var retrialWrapper = new UowStatus(result.Item.CurrentRetrialCount, isInProcessing: false);
            await _repository.DeleteAsync(uowId);
            await _repository.CreateAsync(new CreateKVRequest<UowStatus> { Item = retrialWrapper, Key = uowId });            
        }

        public async Task<bool> IsInProcessing(string uowId)
        {
            var result = await _repository.GetAsync<UowStatus>(uowId);
            return result.IsSuccessfull && result.Item.IsInProcessing;
        }

        public async Task<bool> HasExpiredRetries(string uowId, int maxNumberOfRetries)
        {
            if (maxNumberOfRetries == -1)
                return false;

            var result = await _repository.GetAsync<UowStatus>(uowId);
            if (result.Item == null && !string.IsNullOrEmpty(result.ErrorMessage))            
                throw new InvalidOperationException($"Retrieval of uow status information for {uowId} failed with message: {result.ErrorMessage} and code: {result.ErrorCode}");                               
            
            if (result.Item != null)
            {
                if (result.Item.CurrentRetrialCount < maxNumberOfRetries)
                {
                    await IncrementRetrialCount(uowId, result);
                    return false;
                }
            }
            else
            {
                var retrialWrapper = new UowStatus(currentRetrialCount: 0);
                await _repository.CreateAsync(new CreateKVRequest<UowStatus> { Item = retrialWrapper, Key = uowId });
                return false;
            }

            await _logService.LogInfoAsync(_logId, $"Maximum retrials reached for RingbaUOW by id: {uowId}");
            return true;
        }

        public async Task<bool> HasExpiredAge(string uowId, long creationEPOCH, int maxAgeInSeconds)
        {
            if (maxAgeInSeconds == -1)
                return false;

            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(creationEPOCH);
            if ((DateTime.UtcNow - dateTimeOffset.UtcDateTime).TotalSeconds > maxAgeInSeconds)
            {
                await _logService.LogInfoAsync(_logId, $"Expiry age reached for RingbaUOW by id: {uowId}");
                return true;
            }

            return false;
        }

        public async Task ClearStatusMetadata(string uowId)
        {
            await _repository.DeleteAsync(uowId);
        }
        
        private async Task IncrementRetrialCount(string uowId, Result<UowStatus> result)
        {
            var retrialWrapper = new UowStatus(result.Item.CurrentRetrialCount + 1);
            await _repository.DeleteAsync(uowId);
            await _repository.CreateAsync(new CreateKVRequest<UowStatus> { Item = retrialWrapper, Key = uowId });
        }
    }
}
