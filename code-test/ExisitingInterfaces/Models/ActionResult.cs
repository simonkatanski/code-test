namespace RingbaLibs.Models
{
    public class ActionResult
    {
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }

        /// <summary>
        /// bool value if the action was succesfull
        /// </summary>
        public bool IsSuccessfull { get; set; }
    }
}