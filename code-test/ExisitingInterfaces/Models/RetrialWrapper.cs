namespace code_test.ExisitingInterfaces.Models
{   
    public class RetrialWrapper<T> where T : class
    {
        public RetrialWrapper(T item, int currentRetrialCount, bool isInProcessing = true)
        {
            Item = item;
            CurrentRetrialCount = currentRetrialCount;
            IsInProcessing = isInProcessing;
        }

        public T Item { get; }

        public int CurrentRetrialCount { get; }

        public bool IsInProcessing { get; }
    }
}
