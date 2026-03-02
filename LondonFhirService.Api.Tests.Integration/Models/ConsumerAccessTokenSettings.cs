// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Api.Tests.Integration.Models
{
    public class ConsumerAccessTokenSettings
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string Scope { get; set; } = "";
        public string GrantType { get; set; } = "client_credentials";
        public string AuthorisationUrl { get; set; } = "";
    }
}
