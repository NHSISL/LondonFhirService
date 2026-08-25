// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Manage.Tests.Acceptance.Models.Providers
{
    public class Provider
    {
        public Guid Id { get; set; }
        public string FriendlyName { get; set; }
        public string FullyQualifiedName { get; set; }
        public string FhirVersion { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? ActiveFrom { get; set; }
        public DateTimeOffset? ActiveTo { get; set; }
        public bool IsForComparisonOnly { get; set; }
        public bool IsPrimary { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
