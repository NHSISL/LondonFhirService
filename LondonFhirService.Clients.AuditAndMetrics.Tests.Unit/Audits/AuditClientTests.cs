// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Audits;
using LondonFhirService.Core.Abstractions.Models.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Audits
{
    public partial class AuditClientTests
    {
        private readonly Mock<IAuditService> auditServiceMock;
        private readonly IAuditClient auditClient;

        public AuditClientTests()
        {
            this.auditServiceMock = new Mock<IAuditService>();
            this.auditClient = new AuditClient(auditService: this.auditServiceMock.Object);
        }

        public static TheoryData<Xeption, Xeption> ServiceExceptionMappings()
        {
            var innerException = new NullAuditException(message: "Audit is null.");

            return new TheoryData<Xeption, Xeption>
            {
                {
                    new AuditValidationException("Service validation.", innerException),
                    new AuditClientValidationException(
                        "Audit client validation error occurred, fix errors and try again.",
                        innerException)
                },
                {
                    new AuditDependencyValidationException("Service dependency validation.", innerException),
                    new AuditClientValidationException(
                        "Audit client validation error occurred, fix errors and try again.",
                        innerException)
                },
                {
                    new AuditDependencyException("Service dependency.", innerException),
                    new AuditClientDependencyException(
                        "Audit client dependency error occurred, please contact support.",
                        innerException)
                },
                {
                    new AuditServiceException("Service.", innerException),
                    new AuditClientServiceException(
                        "Audit client service error occurred, fix errors and try again.",
                        innerException)
                }
            };
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();

        private static IAudit CreateRandomAudit() =>
            CreateAuditFiller().Create();

        private static List<IAudit> CreateRandomAudits() =>
            CreateAuditFiller().Create(count: GetRandomNumber())
                .Cast<IAudit>()
                    .ToList();

        private static Filler<TestAudit> CreateAuditFiller()
        {
            var filler = new Filler<TestAudit>();
            filler.Setup().OnType<DateTimeOffset>().Use(GetRandomDateTimeOffset());

            return filler;
        }
    }
}
