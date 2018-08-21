using System;
using System.Threading;
using RingbaLibs;

namespace code_test
{
    public class ImplementMeService : IDisposable
    {
        #region private instance members
        private readonly IKVRepository _repository;
        private readonly ILogService _logservice;
        private readonly IMessageProcessService _processService;
        private readonly IMessageQueService _queService;
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

        public void DoWork()
        {
            //TODO: Batch retrieve messages from the IMessageQuService and process them by 
            //calling IMessageProcessService for each message in the batch. If the message processing fails, place the message
            //back on the que so it will be retried at a later date. However, every message should only be tried the stated number of times and
            //only while the job is in its window. The window is the CreationEPOCH + MaxAgeInSeconds


            //Here at Ringba, we taking logging very seriously, and we log EVERTHING, please use
            //ILogService to log everthing. 

            //NOTES
            //there is a limitation to IMessageQueService, it is not garunteed to deliver a message only once, multiple calls
            //to IMessageQueService can return the same message twice. You must hoever, only process a single message once at a time. 
            //This limitation does have some caviets, the message will never be delived once the message has been marked as completed,
            // only while the message is "in flight"

            //the MessageWrapper Id is the id of the message in the message que, and the UOWId is the unique id of the Unit Of Work to be done
            //by the message processer

        }

        public void Stop()
        {
            //finish the jobs and and stop gathering new ones
            //return when done;
        }

        public void Dispose()
        {
            //cleanup   
        }
    }
}