// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Api.Tests.Integration.Models
{
    public class ConsumerAccessTokenSettings
    {
        public string LfsClientId { get; set; } = "";
        public string LfsClientSecret { get; set; } = "";
        public string LfsScope { get; set; } = "";
        public string LfsGrantType { get; set; } = "client_credentials";
        public string LfsAuthorisationUrl { get; set; } = "";
        public string LfsGetStructuredRecordUrl { get; set; } = "";

        public string DdsClientId { get; set; } = "";
        public string DdsClientSecret { get; set; } = "";
        public string DdsScope { get; set; } = "";
        public string DdsGrantType { get; set; } = "client_credentials";
        public string DdsAuthorisationUrl { get; set; } = "";
        public string DdsGetStructuredRecordUrl { get; set; } = "";

        public string NhsNumber { get; set; } = "";
        public string DateOfBirth { get; set; } = null;
        public bool? DemographicsOnly { get; set; } = null;
        public bool? IncludeInactivePatients { get; set; } = null;
    }
}