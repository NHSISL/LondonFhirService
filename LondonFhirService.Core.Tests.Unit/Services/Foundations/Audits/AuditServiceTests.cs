// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using LondonFhirService.Core.Services.Foundations.Audits;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using ClientExceptions = LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    /// <summary>
    /// This service no longer validates or stamps anything - both moved into the audit and
    /// metrics library, behind the broker. What is left to test is that it forwards faithfully
    /// and that it localises the client's exceptions into this application's own, so callers keep
    /// depending on Core's contract.
    /// </summary>
    public partial class AuditServiceTests
    {
        private readonly Mock<IAuditAndMetricBroker> auditAndMetricBrokerMock;
        private readonly Mock<ISecurityAuditBroker> securityAuditBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IAuditService auditService;

        public AuditServiceTests()
        {
            this.auditAndMetricBrokerMock = new Mock<IAuditAndMetricBroker>();
            this.securityAuditBrokerMock = new Mock<ISecurityAuditBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            // The security broker is what makes CreatedBy/CreatedDate server-side facts rather
            // than request-body claims, so it echoes the entity back by default and individual
            // tests override it when the stamping itself is what is under test.
            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Audit>()))
                    .ReturnsAsync((Audit audit) => audit);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Audit>()))
                    .ReturnsAsync((Audit audit) => audit);

            this.auditService = new AuditService(
                auditAndMetricBroker: this.auditAndMetricBrokerMock.Object,
                securityAuditBroker: this.securityAuditBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        /// <summary>
        /// Each case is a client exception paired with the service exception it must surface as.
        /// </summary>
        public static TheoryData<Xeption, Xeption> ClientExceptionMappings()
        {
            var innerException = new Xeption(message: "Inner.");

            return new TheoryData<Xeption, Xeption>
            {
                {
                    new ClientExceptions.AuditClientValidationException("Client validation.", innerException),
                    new AuditServiceValidationException(
                        "Audit validation errors occurred, please try again.",
                        innerException)
                },
                {
                    new ClientExceptions.AuditClientDependencyException("Client dependency.", innerException),
                    new AuditServiceDependencyException(
                        "Audit dependency error occurred, please contact support.",
                        innerException)
                },
                {
                    new ClientExceptions.AuditClientServiceException("Client service.", innerException),
                    new AuditServiceException(
                        "Audit service error occurred, please contact support.",
                        new FailedAuditServiceException(
                            "Failed audit service error occurred, please contact support.",
                            new ClientExceptions.AuditClientServiceException("Client service.", innerException)))
                }
            };
        }

        private void VerifyNoOtherCallsOnAllBrokers()
        {
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();

        private static Audit CreateRandomAudit() =>
            CreateAuditFiller().Create();

        private static List<Audit> CreateRandomAudits() =>
            CreateAuditFiller().Create(count: GetRandomNumber()).ToList();

        private static IQueryable<Audit> CreateRandomAuditsQueryable() =>
            CreateAuditFiller().Create(count: GetRandomNumber()).AsQueryable();

        private static Filler<Audit> CreateAuditFiller()
        {
            var filler = new Filler<Audit>();
            filler.Setup().OnType<DateTimeOffset>().Use(GetRandomDateTimeOffset());

            return filler;
        }
    }
}
