// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessService : IConsumerAccessService
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
            ValidateOnCheckConsumerAccess(request);

            return await this.consumerAccessBroker
                .CheckConsumerAccessAsync(request, cancellationToken);
        });
    }
}
