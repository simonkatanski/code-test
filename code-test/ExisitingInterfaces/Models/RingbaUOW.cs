using System.Collections.Generic;

namespace RingbaLibs.Models
{
    public sealed class RingbaUOW
    {
        public string UOWId { get; }

        public long CreationEPOCH { get; }

        public int MaxNumberOfRetries { get; }

        public int MaxAgeInSeconds { get; }

    }
}