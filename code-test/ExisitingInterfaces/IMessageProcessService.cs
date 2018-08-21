using System.Collections.Generic;
using System.Threading.Tasks;
using RingbaLibs.Models;

namespace RingbaLibs
{

    public interface IMessageProcessService
    {
        /// <summary>
        /// process the unit of work and returns if succesfull or not
        /// </summary>
        /// <param name="uow"></param>
        /// <returns></returns>
        Task<ActionResult> ProccessMessageAsync(RingbaUOW uow);
    }
}