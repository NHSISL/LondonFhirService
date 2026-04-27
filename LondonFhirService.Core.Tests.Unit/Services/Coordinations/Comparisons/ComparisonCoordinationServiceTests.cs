// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using LondonFhirService.Core.Services.Orchestrations.CompareQueue;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Comparisons
{
    public partial class ComparisonCoordinationServiceTests
    {
        private readonly Mock<ICompareQueueOrchestrationService> compareQueueOrchestrationServiceMock;
        private readonly Mock<IComparisonOrchestrationService> comparisonOrchestrationServiceMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IComparisonCoordinationService comparisonCoordinationService;

        public ComparisonCoordinationServiceTests()
        {
            this.compareQueueOrchestrationServiceMock = new Mock<ICompareQueueOrchestrationService>();
            this.comparisonOrchestrationServiceMock = new Mock<IComparisonOrchestrationService>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.comparisonCoordinationService = new ComparisonCoordinationService(
                compareQueueOrchestrationService: this.compareQueueOrchestrationServiceMock.Object,
                comparisonOrchestrationService: this.comparisonOrchestrationServiceMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                identifierBroker: this.identifierBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static FhirRecord CreateRandomFhirRecord() =>
            CreateFhirRecordFiller().Create();

        private static Filler<FhirRecord> CreateFhirRecordFiller()
        {
            var filler = new Filler<FhirRecord>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(GetRandomDateTimeOffset())
                .OnType<StatusType>().Use(StatusType.Pending);

            return filler;
        }

        private static CompareQueueItem CreateRandomCompareQueueItem()
        {
            FhirRecord randomPrimaryFhirRecord = CreateRandomFhirRecord();
            randomPrimaryFhirRecord.IsPrimarySource = true;

            FhirRecord randomSecondaryFhirRecord = CreateRandomFhirRecord();
            randomSecondaryFhirRecord.IsPrimarySource = false;
            randomSecondaryFhirRecord.CorrelationId = randomPrimaryFhirRecord.CorrelationId;

            var compareQueueItem = new CompareQueueItem();
            compareQueueItem.PrimaryFhirRecord = randomPrimaryFhirRecord;
            compareQueueItem.SecondaryFhirRecord = randomSecondaryFhirRecord;

            return compareQueueItem;
        }

        private static ComparisonResult CreateRandomComparisonResult(string correlationId)
        {
            return new ComparisonResult
            {
                CorrelationId = correlationId,
                DiffCount = GetRandomNumber()
            };
        }

        private static string SerializeComparisonResult(ComparisonResult comparisonResult) =>
            JsonSerializer.Serialize(comparisonResult);

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new CompareQueueOrchestrationValidationException(
                    message: "Compare queue orchestration validation error occurred, please try again.",
                    innerException: innerException),

                new CompareQueueOrchestrationDependencyValidationException(
                    message: "Compare queue orchestration dependency validation error occurred, please try again.",
                    innerException: innerException),

                new ComparisonOrchestrationValidationException(
                    message: "Comparison orchestration validation error occurred, please try again.",
                    innerException: innerException),

                new ComparisonOrchestrationDependencyValidationException(
                    message: "Comparison orchestration dependency validation error occurred, please try again.",
                    innerException: innerException),
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new CompareQueueOrchestrationDependencyException(
                    message: "Compare queue orchestration dependency error occurred, please try again.",
                    innerException: innerException),

                new CompareQueueOrchestrationServiceException(
                    message: "Compare queue orchestration service error occurred, please contact support.",
                    innerException: innerException),

                new ComparisonOrchestrationDependencyException(
                    message: "Comparison orchestration dependency error occurred, please try again.",
                    innerException: innerException),

                new ComparisonOrchestrationServiceException(
                    message: "Comparison orchestration service error occurred, please contact support.",
                    innerException: innerException),
            };
        }
    }
}
