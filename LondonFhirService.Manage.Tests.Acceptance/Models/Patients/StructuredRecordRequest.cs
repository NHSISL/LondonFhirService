// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Tests.Acceptance.Models.Patients
{
    /// <summary>
    /// The suite's own copy of the request contract, as every other model in this project is.
    /// Posting the host's own type would let a rename travel silently from the host into the test
    /// that is supposed to catch it.
    /// </summary>
    public class StructuredRecordRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string GrantType { get; set; }
        public string NhsNumber { get; set; }
        public string DateOfBirth { get; set; }
        public bool DemographicsOnly { get; set; }
    }
}
