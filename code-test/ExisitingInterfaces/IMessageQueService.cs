using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RingbaLibs.Models;

namespace RingbaLibs
{

    public interface IMessageQueService: IDisposable
    {
        Task<MessageBatchResult<T>> GetMessagesFromQueAsync<T>(int maxNumberOfMessages, int emptyQueWaitTimeInSeconds = 30, int maxMessageProcessTimeInSeconds = 30);

        Task<ActionResult> UpdateMessagesAsync(IEnumerable<UpdateBatchRequest> request);
    }

    public class UpdateBatchRequest
    {
        public string Id { get; set; }

        public bool MessageCompleted { get; set; }
    }


    public class MessageBatchResult<T> : ActionResult
    {
        public int NumberOfMessages { get; set; }
        public IEnumerable<MessageWrapper<T>> Messages { get; set; }

    }

    public class MessageWrapper<T>
    {
        public T Body { get; set; }

        public string Id { get; set; }
    }
}

