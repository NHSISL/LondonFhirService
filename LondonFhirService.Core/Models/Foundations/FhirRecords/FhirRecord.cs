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

        /// <summary>
        /// The W3C trace id of the request that produced this record, in the 32 hex digit form
        /// ("N") - the same string the X-Correlation-Id response header carries and the same one
        /// Application Insights files the request under as operation_Id. Rows written before
        /// that convention landed hold the dashed form instead.
        ///
        /// A primary and its secondaries share this value; it is what pairs them in the compare
        /// queue, so both sides are always written by the same request in the same format.
        /// </summary>
        public string CorrelationId { get; set; }
        public string JsonPayload { get; set; }
        public string SourceName { get; set; }
        public bool IsPrimarySource { get; set; }
        public bool IsProcessed { get; set; }
        public StatusType Status { get; set; } = StatusType.Pending;

        /// <summary>
        /// When the row actually became visible to other readers, stamped by the database rather
        /// than by the caller. The compare queue measures how long a secondary has gone without a
        /// sibling primary from this, and CreatedDate/UpdatedDate cannot measure that: they are
        /// stamped on the request thread, while the insert itself happens later on the dispatch
        /// queue. Filtering on those made the wait start counting before the row existed, so the
        /// queue's own latency ate the grace period it was meant to provide.
        /// </summary>
        public DateTimeOffset InsertedDate { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
