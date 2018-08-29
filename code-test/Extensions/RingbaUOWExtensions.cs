using RingbaLibs;
using RingbaLibs.Models;

namespace code_test.Extensions
{
    public static class RingbaUOWExtensions
    {
        public static CreateKVRequest<RingbaUOW> ToKVRequest(this RingbaUOW ringbaUOW)
        {
            return new CreateKVRequest<RingbaUOW>
            {
                ExpireInSeconds = ringbaUOW.MaxAgeInSeconds,
                Item = ringbaUOW,
                Key = ringbaUOW.UOWId
            };
        }
    }
}
