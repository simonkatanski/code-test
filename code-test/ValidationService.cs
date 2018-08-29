using code_test.ExisitingInterfaces.Models;
using code_test.Extensions;
using RingbaLibs;
using RingbaLibs.Models;
using System;
using System.Threading.Tasks;

namespace code_test
{
    public class ValidationService : IValidationService
    {
        private readonly IKVRepository _repository;
        private readonly ILogService _logService;
        private readonly string _logId;

        public ValidationService(
            IKVRepository repository,
            ILogService logService)
        {
            _repository = repository;
            _logService = logService;
            _logId = typeof(ValidationService).Name;
        }

        public async Task ResetInProcessing(RingbaUOW uow)
        {
            var result = await _repository.GetAsync<RetrialWrapper<RingbaUOW>>(uow.UOWId);
            var retrialWrapper = new RetrialWrapper<RingbaUOW>(uow, result.Item.CurrentRetrialCount, isInProcessing: false);
            await _repository.DeleteAsync(uow.UOWId);
            await _repository.CreateAsync(new CreateKVRequest<RetrialWrapper<RingbaUOW>> { Item = retrialWrapper, Key = uow.UOWId });            
        }

        public async Task<bool> IsInProcessing(RingbaUOW uow)
        {
            var result = await _repository.GetAsync<RetrialWrapper<RingbaUOW>>(uow.UOWId);
            return result.IsSuccessfull && result.Item.IsInProcessing;
        }

        public async Task<bool> HasExpiredRetries(RingbaUOW uow)
        {
            if (uow.MaxNumberOfRetries == -1)
                return false;

            var result = await _repository.GetAsync<RetrialWrapper<RingbaUOW>>(uow.UOWId);
            if (result.Item == null && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                await _logService.LogCriticalAsync(_logId, $"Retrieving key value entry failed for RingbaUOW by id: {uow.UOWId} with error code: {result.ErrorCode} and error message: {result.ErrorMessage}");
                return false;
            }

            if (result.Item != null)
            {
                if (result.Item.CurrentRetrialCount < uow.MaxNumberOfRetries)
                {                    
                    var retrialWrapper = new RetrialWrapper<RingbaUOW>(uow, result.Item.CurrentRetrialCount + 1);
                    await _repository.DeleteAsync(uow.UOWId);
                    await _repository.CreateAsync(new CreateKVRequest<RetrialWrapper<RingbaUOW>> { Item = retrialWrapper, Key = uow.UOWId });
                    return false;
                }
            }
            else
            {
                var retrialWrapper = new RetrialWrapper<RingbaUOW>(uow, 0);
                await _repository.CreateAsync(new CreateKVRequest<RetrialWrapper<RingbaUOW>> { Item = retrialWrapper, Key = uow.UOWId });
                return false;
            }

            await _logService.LogInfoAsync(_logId, $"Maximum retrials reached for RingbaUOW by id: {uow.UOWId}");
            return true;
        }

        public async Task<bool> HasExpiredAge(RingbaUOW uow)
        {
            if (uow.MaxAgeInSeconds == -1)
                return false;

            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(uow.CreationEPOCH);
            if ((dateTimeOffset.UtcDateTime - DateTime.UtcNow).Seconds > uow.MaxAgeInSeconds)
            {
                await _logService.LogInfoAsync(_logId, $"Expiry age reached for RingbaUOW by id: {uow.UOWId}");
                return true;
            }

            return false;
        }

        public async Task ClearValidationMetadata(RingbaUOW uow)
        {
            await _repository.DeleteAsync(uow.UOWId);
        }
    }
}
