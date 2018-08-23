using RingbaLibs;
using RingbaLibs.Models;
using System;
using System.Linq;
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

        }

        public async Task DoWork()
        {
            MessageBatchResult<RingbaUOW> batch;
            while ((batch = await _queService.GetMessagesFromQueAsync<RingbaUOW>(
                MaxNumberOfMessages, 
                EmptyQueWaitTimeInSeconds, 
                MaxMessageProcessTimeInSeconds)) != null)
            {                
                var processingTasks = batch.Messages.Select(ProcessSingleMessage);
                var batchResults = await Task.WhenAll(processingTasks);
                await _queService.UpdateMessagesAsync(batchResults);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private async Task<UpdateBatchRequest> ProcessSingleMessage(MessageWrapper<RingbaUOW> message)
        {
            try
            {
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
    }
}
