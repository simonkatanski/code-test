using code_test.Extensions;
using RingbaLibs;
using RingbaLibs.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace code_test
{
    public class ImplementMeService : IDisposable
    {       
        #region private instance members        
        private readonly IUOWStatusService _uowStatusService;
        private readonly ILogService _logService;
        private readonly IMessageProcessService _processService;
        private readonly IMessageQueService _queService;

        private const int MaxNumberOfMessages = 10;
        private const int EmptyQueWaitTimeInSeconds = 30;
        private const int MaxMessageProcessTimeInSeconds = 30;

        private CancellationTokenSource _cts;
        private string _logId;
        #endregion

        public ImplementMeService(
            IUOWStatusService uowStatusService,
            ILogService logService,
            IMessageProcessService messageProcessService,
            IMessageQueService messageQueService)
        {            
            _uowStatusService = uowStatusService;
            _logService = logService;
            _queService = messageQueService;
            _processService = messageProcessService;

            _cts = new CancellationTokenSource();
            _logId = typeof(ImplementMeService).Name;
        }

        public async Task DoWork()
        {
            await _logService.LogInfoAsync(_logId, "Queue processing started.");

            while (!_cts.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                                
                await _logService.LogInfoAsync(_logId, "Getting batch from queue.");

                var batch = await GetMessagesBatch();

                await _logService.LogInfoAsync(_logId, $"Number of message in batch: {batch.NumberOfMessages}");
                await _logService.LogInfoAsync(_logId, "Started messages processing.");
                        
                var batchProcessingTasks = batch.Messages.Select(message => ProcessSingleMessage(message, _cts.Token));
                UpdateBatchRequest[] updateBatchRequests = await Task.WhenAll(batchProcessingTasks);

                var completedMessagesCount = updateBatchRequests.Count(p => p.MessageCompleted);
                var uncompletedMessagesCount = batch.NumberOfMessages - completedMessagesCount;
                await _logService.LogInfoAsync(_logId, $"Finished messages processing. Completed messages: {completedMessagesCount}. Uncompleted messages: {uncompletedMessagesCount}.");

                await _queService.UpdateMessagesAsync(updateBatchRequests);

                sw.Stop();
                await _logService.LogInfoAsync(_logId, $"Batch processing completed, took: {sw.ElapsedMilliseconds} ms.");
            }

            await _logService.LogInfoAsync(_logId, "Queue processing stopped.");
            _cts = GetResetCts();
        }
        
        public void Stop()
        {
            if (!_cts.IsCancellationRequested)
            {
                _logService.LogInfo(_logId, "Stopping queue processing.");
                _cts.Cancel();
            }
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
            await _logService.LogInfoAsync(_logId, $"Processing message by id: {message.Id} started.");
            var uow = message.Body;

            try
            {
                if (await _uowStatusService.IsInProcessing(uow.UOWId))
                    return null;

                if(await _uowStatusService.HasExpiredAge(uow.UOWId, uow.CreationEPOCH, uow.MaxAgeInSeconds) ||
                   await _uowStatusService.HasExpiredRetries(uow.UOWId, uow.MaxNumberOfRetries))
                {
                    await _uowStatusService.ClearStatusMetadata(uow.UOWId);
                    return new UpdateBatchRequest
                    {
                        Id = message.Id,
                        MessageCompleted = true
                    };
                }
                                
                var sw = Stopwatch.StartNew();

                token.ThrowIfCancellationRequested();
                ActionResult result = await _processService.ProccessMessageAsync(message.Body);
                var messageStatusInfo = result.GetStatusInfo();

                sw.Stop();
                await _logService.LogInfoAsync(_logId, $"Processing message by id: {message.Id} completed with message status: {messageStatusInfo}, took: {sw.ElapsedMilliseconds} ms.");

                if (result.IsSuccessfull)
                    await _uowStatusService.ClearStatusMetadata(uow.UOWId);
                else
                    await _uowStatusService.ResetInProcessing(uow.UOWId);

                return new UpdateBatchRequest
                {
                    Id = message.Id,
                    MessageCompleted = result.IsSuccessfull
                };
            }
            catch(OperationCanceledException)
            {
                await _logService.LogInfoAsync(_logId, $"Processing of message by id: {message.Id} has been cancelled.");
            }
            catch (Exception ex) //TODO: Change implementation to only catch exceptions expected from ProcessMessageAsync
            {
                await _logService.LogExceptionAsync(_logId, ex, $"Processing message by id: {message.Id} failed.");                
            }

            await _uowStatusService.ResetInProcessing(uow.UOWId);

            return new UpdateBatchRequest
            {
                Id = message.Id,
                MessageCompleted = false
            };
        }

        private CancellationTokenSource GetResetCts()
        {
            if (_cts != null && _cts.IsCancellationRequested)
                _cts.Dispose();

            return new CancellationTokenSource();
        }
    }
}
