// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessService : IConsumerAccessService
    {
        private readonly IConsumerAccessBroker consumerAccessBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public ConsumerAccessService(
            IConsumerAccessBroker consumerAccessBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.consumerAccessBroker = consumerAccessBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<ConsumerAccess> CheckConsumerAccessAsync(
            string NhsNumber,
            Guid CorrelationId,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            string consumerUserId = await this.securityAuditBroker.GetUserIdAsync();
            await ValidateOnCheckConsumerAccessAsync(consumerUserId, NhsNumber, CorrelationId);

            ValidateAccessRequest validateAccessRequest = new ValidateAccessRequest
            {
                ConsumerUserId = consumerUserId,
                NhsNumber = NhsNumber,
                CorrelationId = CorrelationId
            };

            ConsumerAccess consumerAccess =
                await this.consumerAccessBroker.CheckConsumerAccessAsync(validateAccessRequest);

            ValidateConsumerAccessIsNotNull(consumerAccess);

            return consumerAccess;
        });
    }
}
