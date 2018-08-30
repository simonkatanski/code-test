namespace code_test.ExisitingInterfaces.Models
{   
    public class UowStatus
    {
        public UowStatus(int currentRetrialCount, bool isInProcessing = true)
        {            
            CurrentRetrialCount = currentRetrialCount;
            IsInProcessing = isInProcessing;
        }
                
        public int CurrentRetrialCount { get; }

        public bool IsInProcessing { get; }
    }
}
