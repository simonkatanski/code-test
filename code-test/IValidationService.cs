using RingbaLibs.Models;
using System.Threading.Tasks;

namespace code_test
{
    public interface IValidationService
    {
        Task<bool> HasExpiredRetries(RingbaUOW uow);

        Task<bool> HasExpiredAge(RingbaUOW uow);

        Task<bool> IsInProcessing(RingbaUOW uow);

        Task ClearValidationMetadata(RingbaUOW uow);

        Task ResetInProcessing(RingbaUOW uow);
    }
}