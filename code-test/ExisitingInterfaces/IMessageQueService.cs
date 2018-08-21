using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RingbaLibs.Models;

namespace RingbaLibs
{

    public interface IMessageQueService: IDisposable
    {
        /// <summary>
        /// retreives the messages from the que
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxNumberOfMessages">the max number of messages</param>
        /// <param name="emptyQueWaitTimeInSeconds">how long to wait if the que is empty</param>
        /// <param name="maxMessageProcessTimeInSeconds">how long to hold message out of que until it is put back on the que</param>
        /// <returns>up to maxNumberOfMessages</returns>
        Task<MessageBatchResult<T>> GetMessagesFromQueAsync<T>(int maxNumberOfMessages, int emptyQueWaitTimeInSeconds = 30, int maxMessageProcessTimeInSeconds = 30);

        /// <summary>
        /// updates the message, it either perminently removes message from que or places the message back on the que depending
        /// on the value of the MessageCompleted flag
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

