// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Manage.Controllers.Audits;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Audits
{
    public partial class AuditsControllerTests : RESTFulController
    {

        private readonly Mock<IAuditService> auditServiceMock;
        private readonly AuditsController auditsController;

        public AuditsControllerTests()
        {
            auditServiceMock = new Mock<IAuditService>();
            auditsController = new AuditsController(auditServiceMock.Object);
        }

        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new AuditServiceValidationException(
                    message: someMessage,
                    innerException: someInnerException),

                new AuditServiceDependencyValidationException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        public static TheoryData<Xeption> ServerExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new AuditServiceDependencyException(
                    message: someMessage,
                    innerException: someInnerException),

                new AuditServiceException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Audit CreateRandomAudit() =>
            CreateAuditFiller().Create();

        private static IQueryable<Audit> CreateRandomAudits()
        {
            return CreateAuditFiller()
                .Create(count: GetRandomNumber())
                    .AsQueryable();
        }

        private static Filler<Audit> CreateAuditFiller()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            string user = Guid.NewGuid().ToString();
            var filler = new Filler<Audit>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(audit => audit.AuditType).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(audit => audit.LogLevel).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(audit => audit.CreatedBy).Use(user)
                .OnProperty(audit => audit.UpdatedBy).Use(user);

            return filler;
        }
    }
}
