// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Models.Foundations.Patients
{
    /// <summary>
    /// The endpoints and the fallback client credentials the structured record page calls with.
    /// Bound once from the PatientConfiguration section of appsettings.json and registered as a
    /// singleton, so there is one instance behind every reader.
    ///
    /// The credentials here are a fallback. An operator can type their own into the page - that is
    /// the point of the screen, to prove a consumer's credentials work - and whatever they leave
    /// blank falls back to these.
    /// </summary>
    public class PatientConfiguration
    {
        public string AuthUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string GrantType { get; set; }
        public string GetStructuredRecordUrl { get; set; }
    }
}
