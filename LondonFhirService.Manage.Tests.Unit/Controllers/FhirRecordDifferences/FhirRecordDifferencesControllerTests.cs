// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using LondonFhirService.Core.Services.Foundations.FhirRecordDifferences;
using LondonFhirService.Manage.Controllers;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesControllerTests : RESTFulController
    {

        private readonly Mock<IFhirRecordDifferenceService> fhirRecordDifferenceServiceMock;
        private readonly FhirRecordDifferencesController fhirRecordDifferencesController;

        public FhirRecordDifferencesControllerTests()
        {
            fhirRecordDifferenceServiceMock = new Mock<IFhirRecordDifferenceService>();
            fhirRecordDifferencesController = new FhirRecordDifferencesController(fhirRecordDifferenceServiceMock.Object);
        }

        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new FhirRecordDifferenceValidationException(
                    message: someMessage,
                    innerException: someInnerException),

                new FhirRecordDifferenceDependencyValidationException(
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
                new FhirRecordDifferenceDependencyException(
                    message: someMessage,
                    innerException: someInnerException),

                new FhirRecordDifferenceServiceException(
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

        private static FhirRecordDifference CreateRandomFhirRecordDifference() =>
            CreateFhirRecordDifferenceFiller().Create();

        private static IQueryable<FhirRecordDifference> CreateRandomFhirRecordDifferences()
        {
            return CreateFhirRecordDifferenceFiller()
                .Create(count: GetRandomNumber())
                    .AsQueryable();
        }

        private static Filler<FhirRecordDifference> CreateFhirRecordDifferenceFiller()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            string user = Guid.NewGuid().ToString();
            var filler = new Filler<FhirRecordDifference>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(fhirRecordDifference => fhirRecordDifference.CreatedBy).Use(user)
                .OnProperty(fhirRecordDifference => fhirRecordDifference.UpdatedBy).Use(user);

            return filler;
        }
    }
}