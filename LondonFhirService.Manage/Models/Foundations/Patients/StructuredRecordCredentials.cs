// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Models.Foundations.Patients
{
    /// <summary>
    /// The credentials the token exchange will actually be made with, after each blank field on
    /// the request has fallen back to its PatientConfiguration counterpart.
    ///
    /// It exists so validation and the outbound call read the same four values. Validating the
    /// request alone would reject a caller who deliberately left a field blank to use the
    /// configured one, and validating nothing would send a request with an empty client_id and
    /// report the authorisation server's rejection instead of the missing setting.
    /// </summary>
    internal class StructuredRecordCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string GrantType { get; set; }
    }
}
