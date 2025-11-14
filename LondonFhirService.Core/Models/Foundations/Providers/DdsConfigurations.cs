// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Models.Foundations.Providers
{
    public class DdsConfigurations
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string AuthorisationUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string System { get; set; } = string.Empty;
    }
}
