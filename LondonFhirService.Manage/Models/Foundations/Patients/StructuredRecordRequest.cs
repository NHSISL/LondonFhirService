// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Models.Foundations.Patients
{
    /// <summary>
    /// What the structured record page posts. The four credential fields are optional: each one
    /// falls back to its PatientConfiguration counterpart when the operator leaves it blank, so a
    /// caller can exercise the configured consumer or supply another one without a redeploy.
    ///
    /// No member carries a validation attribute. Every rule this request is held to lives in
    /// PatientService.Validations, so a rejection is one the foundation states rather than one
    /// model binding invented before the service was reached.
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
