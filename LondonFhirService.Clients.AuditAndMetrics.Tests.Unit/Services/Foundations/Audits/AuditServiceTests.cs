// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.DateTimes;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Identifiers;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings;
using LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Audits;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Abstractions.Models.Audits;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        private readonly Mock<IAuditBroker> auditBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IAuditUserBroker> auditUserBrokerMock;
        private readonly Mock<IAuditAndMetricsDispatcher> dispatcherMock;
        private readonly IAuditService auditService;

        public AuditServiceTests()
        {
            this.auditBrokerMock = new Mock<IAuditBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.auditUserBrokerMock = new Mock<IAuditUserBroker>();
            this.dispatcherMock = new Mock<IAuditAndMetricsDispatcher>();

            // Runs the deferred work inline. The production dispatcher hands it to a queue, which
            // would make every dispatched-path assertion a race; here the write has happened by
            // the time the call returns.
            this.dispatcherMock.Setup(dispatcher =>
                dispatcher.TryDispatch(It.IsAny<Func<CancellationToken, ValueTask>>()))
                    .Returns((Func<CancellationToken, ValueTask> work) =>
                    {
                        work(CancellationToken.None).AsTask().GetAwaiter().GetResult();

                        return true;
                    });

            // The library holds no implementation of IAudit, so the entity it builds comes back
            // through the port - exactly as it does from the hosting application at runtime.
            this.auditBrokerMock.Setup(broker => broker.CreateAudit())
                .Returns(() => new TestAudit());

            this.auditService = new AuditService(
                auditBroker: this.auditBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                identifierBroker: this.identifierBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                auditUserBroker: this.auditUserBrokerMock.Object,
                dispatcher: this.dispatcherMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static Guid GetRandomGuid() =>
            Guid.NewGuid();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();

        /// <summary>
        /// Valid but unstamped: everything ValidateAuditOnAdd requires except the CreatedDate and
        /// the user, which are what the stamping tests are about.
        /// </summary>
        private static TestAudit CreateUnstampedAudit() =>
            new TestAudit
            {
                Id = GetRandomGuid(),
                AuditType = GetRandomString(),
                Title = GetRandomString(),
                Message = GetRandomString(),
                FileName = GetRandomString(),
                CorrelationId = GetRandomString(),
                LogLevel = "Information"
            };

        private static List<IAudit> CreateUnstampedAudits(int count) =>
            Enumerable.Range(0, count)
                .Select(_ => (IAudit)CreateUnstampedAudit())
                    .ToList();
    }
}
