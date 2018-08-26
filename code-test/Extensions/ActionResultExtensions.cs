using RingbaLibs.Models;

namespace code_test.Extensions
{
    public static class ActionResultExtensions
    {
        public static string GetStatusInfo(this ActionResult result)
        {
            if (!result.IsSuccessfull)
                return $"IsSuccessful: {result.IsSuccessfull}, Error code: {result.ErrorCode}, Error message: {result.ErrorMessage}";

            return $"IsSuccessful: {result.IsSuccessfull}";
        }
    }
}
