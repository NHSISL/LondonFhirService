// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LondonFhirService.Core.Models.Brokers.ConsumerAccesses
{
    public class ConsumerAccess
    {
        [JsonPropertyName("nhsNumber")]
        public string NhsNumber { get; set; } = string.Empty;

        [JsonPropertyName("consumerId")]
        public string ConsumerId { get; set; } = string.Empty;

        [JsonPropertyName("consumerOrgCode")]
        public string ConsumerOrgCode { get; set; } = string.Empty;

        [JsonPropertyName("isAccessAllowed")]
        public bool IsAccessAllowed { get; set; }

        [JsonPropertyName("allowedViaInformationSharingAgreements")]
        public List<string> AllowedViaInformationSharingAgreements { get; set; } = new();

        [JsonPropertyName("allowedViaOrganisations")]
        public List<string> AllowedViaOrganisations { get; set; } = new();

        [JsonPropertyName("reasons")]
        public List<AccessReason> Reasons { get; set; } = new();

        [JsonPropertyName("correlationId")]
        public Guid CorrelationId { get; set; }
    }
}
