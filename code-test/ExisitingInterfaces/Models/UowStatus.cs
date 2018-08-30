namespace code_test.ExisitingInterfaces.Models
{       
    public class UowStatus
    {
        public UowStatus(int currentRetrialCount, bool isInProcessing = true)
        {            
            CurrentRetrialCount = currentRetrialCount;
            IsInProcessing = isInProcessing;
        }
    
        /// <summary>
        /// The current retrial number, how many given uow has been retried.
        /// </summary>
        public int CurrentRetrialCount { get; }

        /// <summary>
        /// The status of the uow in processing. When True, means that this uow is currently already being processed.
        /// </summary>
        public bool IsInProcessing { get; }
    }
}
