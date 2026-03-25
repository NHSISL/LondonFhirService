// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using LondonFhirService.Core.Services.Foundations.FhirRecords;
using LondonFhirService.Manage.Controllers;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests : RESTFulController
    {

        private readonly Mock<IFhirRecordService> fhirRecordServiceMock;
        private readonly FhirRecordsController fhirRecordsController;

        public FhirRecordsControllerTests()
        {
            fhirRecordServiceMock = new Mock<IFhirRecordService>();
            fhirRecordsController = new FhirRecordsController(fhirRecordServiceMock.Object);
        }

        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new FhirRecordValidationException(
                    message: someMessage,
                    innerException: someInnerException),

                new FhirRecordDependencyValidationException(
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
                new FhirRecordDependencyException(
                    message: someMessage,
                    innerException: someInnerException),

                new FhirRecordServiceException(
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

        private static FhirRecord CreateRandomFhirRecord() =>
            CreateFhirRecordFiller().Create();

        private static IQueryable<FhirRecord> CreateRandomFhirRecords()
        {
            return CreateFhirRecordFiller()
                .Create(count: GetRandomNumber())
                    .AsQueryable();
        }

        private static Filler<FhirRecord> CreateFhirRecordFiller()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            string user = Guid.NewGuid().ToString();
            var filler = new Filler<FhirRecord>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(fhirRecord => fhirRecord.CreatedBy).Use(user)
                .OnProperty(fhirRecord => fhirRecord.UpdatedBy).Use(user);

            return filler;
        }
    }
}