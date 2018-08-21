using System.Collections.Generic;

namespace RingbaLibs.Models
{
    public sealed class RingbaUOW
    {
        /// <summary>
        /// the unique id of the Unit of Work
        /// </summary>
        public string UOWId { get; }

        /// <summary>
        /// the epoch this Unit of Work was created
        /// </summary>
        public long CreationEPOCH { get; }

        /// <summary>
        /// the Max number of tries this Unit of Work can be processed
        /// </summary>
        public int MaxNumberOfRetries { get; }

        /// <summary>
        /// the max age in seconds this unit of Work is Valid for
        /// </summary>
        public int MaxAgeInSeconds { get; }

    }
}