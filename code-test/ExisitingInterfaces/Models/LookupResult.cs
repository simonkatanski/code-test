namespace RingbaLibs.Models
{
    public class Result<T> : ActionResult
    {
        public T Item { get; set; }
    }
}