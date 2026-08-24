// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessServiceTests
    {
        private readonly Mock<IConsumerAccessBroker> consumerAccessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ConsumerAccessService consumerAccessService;

        public ConsumerAccessServiceTests()
        {
            this.consumerAccessBrokerMock = new Mock<IConsumerAccessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.consumerAccessService = new ConsumerAccessService(
                consumerAccessBroker: this.consumerAccessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static ValidateAccessRequest CreateRandomValidateAccessRequest() =>
            new ValidateAccessRequest
            {
                ConsumerUserId = GetRandomString(),
                NhsNumber = GetRandomString(),
                CorrelationId = Guid.NewGuid()
            };

        private static ConsumerAccess CreateRandomConsumerAccess() =>
            new ConsumerAccess
            {
                NhsNumber = GetRandomString(),
                ConsumerId = GetRandomString(),
                ConsumerOrgCode = GetRandomString(),
                IsAccessAllowed = true,
                AllowedViaInformationSharingAgreements = CreateRandomStrings(),
                AllowedViaOrganisations = CreateRandomStrings(),

                Reasons = new List<AccessReason>
                {
                    new AccessReason { Code = GetRandomString(), Message = GetRandomString() }
                },

                CorrelationId = Guid.NewGuid()
            };

        private static List<string> CreateRandomStrings() =>
            Enumerable.Range(start: 1, count: GetRandomNumber())
                .Select(_ => GetRandomString()).ToList();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(
            Xeption expectedException)
        {
            return actualException =>
                actualException.SameExceptionAs(expectedException);
        }

        public static TheoryData<Exception> DependencyExceptions()
        {
            string randomMessage = GetRandomString();

            return new TheoryData<Exception>
            {
                new HttpRequestException(randomMessage),
                new HttpRequestException(randomMessage, new Exception(randomMessage))
            };
        }

        public static TheoryData<Exception> TimeoutExceptions()
        {
            string randomMessage = GetRandomString();

            return new TheoryData<Exception>
            {
                new TimeoutException(randomMessage),

                new TaskCanceledException(
                    message: randomMessage,
                    innerException: new TimeoutException(randomMessage)),

                new OperationCanceledException(
                    message: randomMessage,
                    innerException: new TimeoutException(randomMessage))
            };
        }

        public static TheoryData<Exception> CancellationExceptions()
        {
            string randomMessage = GetRandomString();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            return new TheoryData<Exception>
            {
                new OperationCanceledException(randomMessage),
                new OperationCanceledException(cancellationTokenSource.Token),
                new TaskCanceledException(randomMessage)
            };
        }
    }
}
