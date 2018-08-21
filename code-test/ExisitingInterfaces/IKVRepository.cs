using System.Threading.Tasks;
using RingbaLibs.Models;

namespace RingbaLibs
{
    public interface IKVRepository
    {
        Task<Result<T>> GetAsync<T>(string key);

        Task<ActionResult> CreateAsync<T>(CreateKVRequest<T> item);

        Task<Result<bool>> CreateIfNotExistAsync<T>(CreateKVRequest<T> item);

        Task<ActionResult> DeleteAsync(string key);


    }

    public class CreateKVRequest<T>
    {
        public string Key { get; set; }

        public T Item { get; set; }

        public int ExpireInSeconds { get; set; } = -1;

    }


}