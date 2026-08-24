// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Abstractions.Models;

namespace LondonFhirService.Core.Models.Foundations.FhirRecords
{
    public class FhirRecord : IKey, IAuditable
    {
        public Guid Id { get; set; }
        public string CorrelationId { get; set; }
        public string JsonPayload { get; set; }
        public string SourceName { get; set; }
        public bool IsPrimarySource { get; set; }
        public bool IsProcessed { get; set; }
        public StatusType Status { get; set; } = StatusType.Pending;

        /// <summary>
        /// When the row actually became visible to other readers, stamped by the database rather
        /// than by the caller. The compare queue waits a buffer period before claiming a secondary
        /// so its sibling primary has time to land, and CreatedDate/UpdatedDate cannot measure
        /// that: they are stamped on the request thread, while the insert itself happens later on
        /// the dispatch queue. Filtering on those made the buffer start counting before the row
        /// existed, so the queue's own latency ate the grace period it was meant to provide.
        /// </summary>
        public DateTimeOffset InsertedDate { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
