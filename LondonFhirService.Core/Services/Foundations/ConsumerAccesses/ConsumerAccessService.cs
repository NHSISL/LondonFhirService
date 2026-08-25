// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
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

            ConsumerAccess maybeConsumerAccess = await this.consumerAccessBroker
                .CheckConsumerAccessAsync(request, cancellationToken);

            // The response is a third party's, so it is checked here rather than dereferenced
            // upstream. A 2xx carrying the literal JSON null deserialises to null, and an explicit
            // null list overwrites the model's initialisers - either one used to surface as a
            // NullReferenceException in the orchestration, which lost the compliance audit for
            // that access decision on the way past.
            ValidateConsumerAccessResponse(maybeConsumerAccess);

            maybeConsumerAccess.Reasons ??= new List<AccessReason>();
            maybeConsumerAccess.AllowedViaOrganisations ??= new List<string>();
            maybeConsumerAccess.AllowedViaInformationSharingAgreements ??= new List<string>();

            return maybeConsumerAccess;
        });
    }
}
