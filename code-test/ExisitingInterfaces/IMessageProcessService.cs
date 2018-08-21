using System.Collections.Generic;
using System.Threading.Tasks;
using RingbaLibs.Models;

namespace RingbaLibs
{

    public interface IMessageProcessService
    {
        Task<ActionResult> ProccessMessageAsync(RingbaUOW uow);
    }
}