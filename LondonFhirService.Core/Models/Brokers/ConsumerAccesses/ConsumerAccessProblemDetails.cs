// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json.Serialization;

namespace LondonFhirService.Core.Models.Brokers.ConsumerAccesses
{
    /// <summary>
    /// The application/problem+json body ConsumerAccessService sends with every 401 and with a 403
    /// that is not an access decision. ErrorCode is what tells its refusals apart: ConsumerUnknown
    /// is about the consumer; BearerTokenMissing, BearerTokenInvalid and InsufficientPermissions are
    /// about this service's own credentials.
    /// </summary>
    public class ConsumerAccessProblemDetails
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; }
    }
}
