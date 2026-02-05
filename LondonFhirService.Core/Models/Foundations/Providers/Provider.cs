// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Bases;

namespace LondonFhirService.Core.Models.Foundations.Providers
{
    public class Provider : IKey, IAudit
    {
        public Guid Id { get; set; }
        public string FriendlyName { get; set; } = string.Empty;
        public string FullyQualifiedName { get; set; } = string.Empty;
        public string FhirVersion { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset? ActiveFrom { get; set; }
        public DateTimeOffset? ActiveTo { get; set; }
        public bool IsForComparisonOnly { get; set; }
        public bool IsPrimary { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
