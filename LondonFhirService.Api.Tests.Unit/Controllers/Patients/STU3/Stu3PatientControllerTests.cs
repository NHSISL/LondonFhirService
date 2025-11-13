// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Hl7.Fhir.Model;
using LondonFhirService.Api.Controllers;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.STU3
{
    public partial class Stu3PatientControllerTests : RESTFulController
    {
        private readonly Mock<IStu3PatientCoordinationService> patientCoordinationServiceMock;
        private readonly Stu3PatientController patientController;

        public Stu3PatientControllerTests()
        {
            this.patientCoordinationServiceMock = new Mock<IStu3PatientCoordinationService>();

            this.patientController = new Stu3PatientController(
                this.patientCoordinationServiceMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static Bundle CreateRandomBundle() =>
            new Bundle
            {
                Id = GetRandomString(),
                Type = Bundle.BundleType.Searchset,
                Total = GetRandomNumber()
            };

        private static Parameters CreateRandomParameters(
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            var parameters = new Parameters();

            if (start.HasValue)
            {
                parameters.Add("start", new FhirDateTime(start.Value));
            }

            if (end.HasValue)
            {
                parameters.Add("end", new FhirDateTime(end.Value));
            }

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                parameters.Add("_type", new FhirString(typeFilter));
            }

            if (since.HasValue)
            {
                parameters.Add("_since", new FhirDateTime(since.Value));
            }

            if (count.HasValue)
            {
                parameters.Add("_count", new Integer(count.Value));
            }

            return parameters;
        }

        private static Parameters CreateRandomGetStructuredRecordParameters(
            DateTimeOffset? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            var parameters = new Parameters();

            if (dateOfBirth.HasValue)
            {
                parameters.Add("dateOfBirth", new FhirDateTime(dateOfBirth.Value));
            }

            if (demographicsOnly.HasValue)
            {
                parameters.Add("demographicsOnly", new FhirBoolean(demographicsOnly.Value));
            }

            if (includeInactivePatients.HasValue)
            {
                parameters.Add("includeInactivePatients", new FhirBoolean(includeInactivePatients.Value));
            }

            return parameters;
        }

        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new PatientCoordinationValidationException(
                    message: someMessage,
                    innerException: someInnerException),

                new PatientCoordinationDependencyValidationException(
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
                new PatientCoordinationDependencyException(
                    message: someMessage,
                    innerException: someInnerException),

                new PatientCoordinationServiceException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }
    }
}
