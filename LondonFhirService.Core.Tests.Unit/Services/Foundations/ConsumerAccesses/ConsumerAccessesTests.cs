// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq.Expressions;
using LondonFhirService.Core.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        private readonly Mock<IConsumerAccessBroker> consumerAccessBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ISecurityAuditBroker> securityAuditBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ConsumerAccessService consumerAccessService;

        public ConsumerAccessesTests()
        {
            this.consumerAccessBrokerMock = new Mock<IConsumerAccessBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.securityAuditBrokerMock = new Mock<ISecurityAuditBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.consumerAccessService = new ConsumerAccessService(
                consumerAccessBroker: this.consumerAccessBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                securityAuditBroker: this.securityAuditBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static ConsumerAccess CreateRandomConsumerAccess() =>
            CreateConsumerAccessFiller().Create();

        private static Filler<ConsumerAccess> CreateConsumerAccessFiller()
        {
            var filler = new Filler<ConsumerAccess>();
            filler.Setup();

            return filler;
        }

        private static Expression<Func<ValidateAccessRequest, bool>> SameValidateAccessRequestAs(
            ValidateAccessRequest expectedRequest)
        {
            return actualRequest =>
                actualRequest.ConsumerUserId == expectedRequest.ConsumerUserId
                && actualRequest.NhsNumber == expectedRequest.NhsNumber
                && actualRequest.CorrelationId == expectedRequest.CorrelationId;
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Guid GetRandomGuid() =>
            Guid.NewGuid();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(
            Xeption expectedException)
        {
            return actualException =>
                actualException.SameExceptionAs(expectedException);
        }
    }
}
