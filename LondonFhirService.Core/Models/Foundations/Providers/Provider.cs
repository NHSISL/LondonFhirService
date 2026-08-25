// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Abstractions.Models;

namespace LondonFhirService.Core.Models.Foundations.Providers
{
    public class Provider : IKey, IAuditable
    {
        public Guid Id { get; set; }
        public string FriendlyName { get; set; } = string.Empty;
        public string FullyQualifiedName { get; set; } = string.Empty;
        public string FhirVersion { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset? ActiveFrom { get; set; }
        public DateTimeOffset? ActiveTo { get; set; }
        /// <summary>
        /// Marks a provider whose data is fetched for comparison only. Not yet read by anything -
        /// it is reserved for the consolidation step: a provider flagged this way will still be
        /// fanned out to, and its bundle still persisted and diffed, but its resources will not be
        /// merged into the consolidated record returned to the consumer.
        ///
        /// Do NOT implement it by excluding the provider from the fan out. A comparison-only
        /// provider has to be called, or the compare queue has nothing to compare. The exclusion
        /// belongs in consolidation, not in provider selection.
        /// </summary>
        public bool IsForComparisonOnly { get; set; }

        public bool IsPrimary { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
