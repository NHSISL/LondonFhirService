// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    internal partial class ConsumerAccessService : IConsumerAccessService
    {
        private readonly IConsumerAccessBroker consumerAccessBroker;
        private readonly ILoggingBroker loggingBroker;

        public ConsumerAccessService(
            IConsumerAccessBroker consumerAccessBroker,
            ILoggingBroker loggingBroker)
        {
            this.consumerAccessBroker = consumerAccessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<ConsumerAccess> CheckConsumerAccessAsync(
            ValidateAccessRequest request,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateOnCheckConsumerAccess(request);

            ConsumerAccessResponse maybeConsumerAccessResponse = await this.consumerAccessBroker
                .CheckConsumerAccessAsync(request, cancellationToken);

            // The dependency answers in more than one status. 401 means it does not know the
            // consumer, and its body is problem details rather than an Access, so it is localised
            // before anything tries to read one.
            ValidateConsumerAccessResponseIsAnswered(maybeConsumerAccessResponse);
            ConsumerAccess maybeConsumerAccess = DeserialiseConsumerAccess(maybeConsumerAccessResponse);

            // The response is a third party's, so it is checked here rather than dereferenced
            // upstream. A 2xx carrying the literal JSON null deserialises to null, and an explicit
            // null list overwrites the model's initialisers - either one used to surface as a
            // NullReferenceException in the orchestration, which lost the compliance audit for
            // that access decision on the way past.
            ValidateConsumerAccessResponse(maybeConsumerAccess);

            maybeConsumerAccess.Reasons ??= new List<AccessReason>();
            maybeConsumerAccess.AllowedViaOrganisations ??= new List<string>();
            maybeConsumerAccess.AllowedViaInformationSharingAgreements ??= new List<string>();

            // A 403 carries the same Access body a 200 does, refusing - so it is returned just as
            // a 200 with IsAccessAllowed false would be, and the orchestration audits the denial
            // as it always has. The status is the decision: a refusal whose body claims otherwise
            // fails closed rather than being read as permission.
            maybeConsumerAccess.IsAccessAllowed =
                maybeConsumerAccess.IsAccessAllowed
                    && maybeConsumerAccessResponse.StatusCode is not HttpStatusCode.Forbidden;

            return maybeConsumerAccess;
        });

        /// <summary>
        /// Read with the web defaults, which is what ReadFromJsonAsync used when the broker read
        /// the body itself. It no longer can: which shape the body has depends on the status, and
        /// telling the statuses apart is this service's call.
        /// </summary>
        private static ConsumerAccess DeserialiseConsumerAccess(ConsumerAccessResponse consumerAccessResponse) =>
            JsonSerializer.Deserialize<ConsumerAccess>(consumerAccessResponse.Content, JsonSerializerOptions.Web);
    }
}
