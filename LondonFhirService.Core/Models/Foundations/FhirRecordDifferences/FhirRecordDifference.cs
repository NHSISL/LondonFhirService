// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Abstractions.Models;

namespace LondonFhirService.Core.Models.Foundations.FhirRecordDifferences
{
    public class FhirRecordDifference : IKey, IAuditable
    {
        public Guid Id { get; set; }
        public Guid PrimaryId { get; set; }
        public Guid SecondaryId { get; set; }
        public string CorrelationId { get; set; }
        /// <summary>
        /// The two records this row compared. They exist so a caller can ask for the source that
        /// produced each side without fetching the record itself - a FhirRecord carries the whole
        /// bundle in JsonPayload, so a list that wanted nothing but a source name was otherwise
        /// pulling a patient record per row to get it.
        ///
        /// Mapped, so OData can $expand them and EF can translate that into a join. A projected
        /// expand - $expand=secondary($select=sourceName,isPrimarySource) - leaves JsonPayload
        /// behind, which is the whole point of reaching them this way.
        /// </summary>
        public FhirRecord Primary { get; set; }

        public FhirRecord Secondary { get; set; }

        public string DiffJson { get; set; }
        public int DiffCount { get; set; }
        public int AcceptableDiffCount { get; set; }
        public DateTimeOffset ComparedAt { get; set; }
        public string Comment { get; set; }
        public bool IsResolved { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
