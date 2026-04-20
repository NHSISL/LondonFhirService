// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq.Expressions;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Services.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Services.Foundations.FhirRecords;
using LondonFhirService.Core.Services.Orchestrations.CompareQueue;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        private readonly Mock<IFhirRecordService> fhirRecordServiceMock;
        private readonly Mock<IFhirRecordDifferenceService> fhirRecordDifferenceServiceMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ICompareQueueOrchestrationService compareQueueOrchestrationService;

        public CompareQueueOrchestrationServiceTests()
        {
            this.fhirRecordServiceMock = new Mock<IFhirRecordService>();
            this.fhirRecordDifferenceServiceMock = new Mock<IFhirRecordDifferenceService>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.compareQueueOrchestrationService = new CompareQueueOrchestrationService(
                fhirRecordService: this.fhirRecordServiceMock.Object,
                fhirRecordDifferenceService: this.fhirRecordDifferenceServiceMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static FhirRecord CreateRandomFhirRecord(DateTimeOffset dateTimeOffset) =>
            CreateFhirRecordFiller(dateTimeOffset).Create();

        private static FhirRecord CreateRandomFhirRecord() =>
            CreateFhirRecordFiller(GetRandomDateTimeOffset()).Create();

        private static Filler<FhirRecord> CreateFhirRecordFiller(DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<FhirRecord>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<StatusType>().Use(StatusType.Pending);

            return filler;
        }

        private static FhirRecordDifference CreateRandomFhirRecordDifference() =>
            CreateFhirRecordDifferenceFiller(GetRandomDateTimeOffset()).Create();

        private static Filler<FhirRecordDifference> CreateFhirRecordDifferenceFiller(
            DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<FhirRecordDifference>();
            filler.Setup().OnType<DateTimeOffset>().Use(dateTimeOffset);

            return filler;
        }

        private static CompareQueueItem CreateRandomCompareQueueItem()
        {
            FhirRecord randomPrimaryFhirRecord = CreateRandomFhirRecord();
            randomPrimaryFhirRecord.IsPrimarySource = true;

            FhirRecord randomSecondaryFhirRecord = CreateRandomFhirRecord();
            randomSecondaryFhirRecord.IsPrimarySource = false;
            randomSecondaryFhirRecord.CorrelationId = randomPrimaryFhirRecord.CorrelationId;

            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();

            var compareQueueItem = new CompareQueueItem();
            compareQueueItem.PrimaryFhirRecord = randomPrimaryFhirRecord;
            compareQueueItem.SecondaryFhirRecord = randomSecondaryFhirRecord;
            compareQueueItem.FhirRecordDifference = randomFhirRecordDifference;

            return compareQueueItem;
        }

        public static TheoryData<Xeption> FhirRecordDependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new FhirRecordValidationException(
                    message: "Fhir record validation errors occurred, please try again.",
                    innerException: innerException),

                new FhirRecordDependencyValidationException(
                    message: "Fhir record dependency validation errors occurred, please try again.",
                    innerException: innerException)
            };
        }

        public static TheoryData<Xeption> FhirRecordDependencyExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new FhirRecordDependencyException(
                    message: "Fhir record dependency errors occurred, please try again.",
                    innerException: innerException),

                new FhirRecordServiceException(
                    message: "Fhir record service errors occurred, please try again.",
                    innerException: innerException)
            };
        }

        public static TheoryData<Xeption> FhirRecordDifferenceDependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new FhirRecordDifferenceValidationException(
                    message: "Fhir record difference validation errors occurred, please try again.",
                    innerException: innerException),

                new FhirRecordDifferenceDependencyValidationException(
                    message: "Fhir record difference dependency validation errors occurred, please try again.",
                    innerException: innerException)
            };
        }

        public static TheoryData<Xeption> FhirRecordDifferenceDependencyExceptions()
        {
            string randomMessage = GetRandomString();
            var innerException = new Xeption(randomMessage);

            return new TheoryData<Xeption>
            {
                new FhirRecordDifferenceDependencyException(
                    message: "Fhir record difference dependency errors occurred, please try again.",
                    innerException: innerException),

                new FhirRecordDifferenceServiceException(
                    message: "Fhir record difference service errors occurred, please try again.",
                    innerException: innerException)
            };
        }
    }
}
