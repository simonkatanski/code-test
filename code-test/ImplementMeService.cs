using RingbaLibs;
using RingbaLibs.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace code_test
{
    public class ImplementMeService : IDisposable
    {       
        #region private instance members
        private readonly IKVRepository _repository;
        private readonly ILogService _logservice;
        private readonly IMessageProcessService _processService;
        private readonly IMessageQueService _queService;

        private const int MaxNumberOfMessages = 10;
        private const int EmptyQueWaitTimeInSeconds = 30;
        private const int MaxMessageProcessTimeInSeconds = 30;

        private CancellationTokenSource _cts;

        #endregion

        public ImplementMeService(
            IKVRepository repository,
            ILogService logService,
            IMessageProcessService messageProcessService,
            IMessageQueService messageQueService)
        {
            _repository = repository;
            _logservice = logService;
            _queService = messageQueService;
            _processService = messageProcessService;

            _cts = new CancellationTokenSource();
        }

        public async Task DoWork()
        {   
            while (!_cts.IsCancellationRequested)
            {
                var batch = await GetMessagesBatch();
                var batchProcessingTasks = batch.Messages.Select(message => ProcessSingleMessage(message, _cts.Token));
                UpdateBatchRequest[] updateBatchRequests = await Task.WhenAll(batchProcessingTasks);
                await _queService.UpdateMessagesAsync(updateBatchRequests);                                
            }
                        
            _cts = GetResetCts();
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
        
        private async Task<MessageBatchResult<RingbaUOW>> GetMessagesBatch() => await _queService.GetMessagesFromQueAsync<RingbaUOW>(
            MaxNumberOfMessages,
            EmptyQueWaitTimeInSeconds,
            MaxMessageProcessTimeInSeconds);

        private async Task<UpdateBatchRequest> ProcessSingleMessage(MessageWrapper<RingbaUOW> message, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var result = await _processService.ProccessMessageAsync(message.Body);
                return new UpdateBatchRequest
                {
                    Id = message.Id,
                    MessageCompleted = result.IsSuccessfull
                };
            }
            catch (Exception ex)
            {
                return new UpdateBatchRequest
                {
                    Id = message.Id,
                    MessageCompleted = false
                };
            }
        }

        private CancellationTokenSource GetResetCts()
        {
            if (_cts != null && _cts.IsCancellationRequested)
                _cts.Dispose();

            return new CancellationTokenSource();
        }
    }
}
