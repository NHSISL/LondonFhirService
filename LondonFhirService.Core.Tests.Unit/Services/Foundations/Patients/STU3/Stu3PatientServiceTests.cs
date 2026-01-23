// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.Fhirs.STU3;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Capabilities;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Exceptions;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using AdministrativeGender = Hl7.Fhir.Model.AdministrativeGender;
using Patient = Hl7.Fhir.Model.Patient;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        private readonly Mock<IFhirAbstractionProvider> fhirAbstractionProviderMock;
        private readonly Mock<IFhirProvider> ddsFhirProviderMock;
        private readonly Mock<IFhirProvider> ldsFhirProviderMock;
        private readonly Mock<IFhirProvider> unsupportedFhirProviderMock;
        private readonly Mock<IFhirProvider> unsupportedErrorFhirProviderMock;
        private readonly Stu3FhirBroker fhirBroker;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IAuditBroker> auditBrokerMock;
        private readonly Mock<IIdentifierBroker> identifierBrokerMock;
        private readonly PatientServiceConfig patientServiceConfig;
        private readonly Stu3PatientService patientService;
        private readonly FhirJsonDeserializer fhirJsonDeserializer = new();
        private readonly FhirJsonSerializer fhirJsonSerializer = new();

        public Stu3PatientServiceTests()
        {
            this.fhirAbstractionProviderMock = new Mock<IFhirAbstractionProvider>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.auditBrokerMock = new Mock<IAuditBroker>();
            this.identifierBrokerMock = new Mock<IIdentifierBroker>();

            this.patientServiceConfig = new PatientServiceConfig
            {
                MaxProviderWaitTimeMilliseconds = 3000
            };

            this.ddsFhirProviderMock = MakeProvider("DDS", "DDS Provider", ("Patients", new[] { "EverythingAsync", "GetStructuredRecordAsync" }));
            this.ldsFhirProviderMock = MakeProvider("LDS", "LDS Provider", ("Patients", new[] { "EverythingAsync", "GetStructuredRecordAsync" }));
            this.unsupportedFhirProviderMock = MakeProvider("Unsupported", "Unsupported Provider", ("Patients", new[] { "Read" }));
            this.unsupportedErrorFhirProviderMock = MakeProvider("UnsupportedError", "Unsupported Error Provider", ("Patients", null));

            var fhirProviders = new List<IFhirProvider>
            {
                ddsFhirProviderMock.Object,
                ldsFhirProviderMock.Object,
                unsupportedFhirProviderMock.Object,
                unsupportedErrorFhirProviderMock.Object
            };

            this.fhirAbstractionProviderMock.SetupGet(provider => provider.FhirProviders)
                .Returns(fhirProviders);

            this.fhirBroker = new Stu3FhirBroker(fhirAbstractionProviderMock.Object);

            this.patientService = new Stu3PatientService(
                fhirBroker: this.fhirBroker,
                auditBroker: this.auditBrokerMock.Object,
                identifierBroker: this.identifierBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                patientServiceConfig: this.patientServiceConfig);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static Patient CreateRandomPatient()
        {
            var patient = new Patient();

            HumanName humanName = new HumanName
            {
                ElementId = GetRandomString(),
                Family = GetRandomString(),
                Given = new List<string> { GetRandomString() },
                Prefix = new List<string> { "Mr" },
                Use = HumanName.NameUse.Usual
            };

            patient.Name = new List<HumanName> { humanName };
            patient.Gender = AdministrativeGender.Male;
            patient.BirthDate = GetRandomString();

            return patient;
        }

        private Bundle CreateRandomBundle()
        {
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Total = 1,
                Timestamp = DateTimeOffset.UtcNow
            };

            Patient patient = CreateRandomPatient();

            bundle.Entry = new List<Bundle.EntryComponent> {
                new Bundle.EntryComponent
                {
                    FullUrl = $"https://api.service.nhs.uk/personal-demographics/FHIR/STU3/Patient/{patient.Id}",
                    Search = new Bundle.SearchComponent { Score = 1 },
                    Resource = patient
                }
            };

            bundle.Meta = new Meta
            {
                LastUpdated = DateTimeOffset.UtcNow,
                Source = GetRandomString()
            };


            return bundle;
        }

        private Mock<IFhirProvider> MakeProvider(
            string providerName,
            string providerDisplayName,
            params (string Resource, string[] Operations)[] resources)
        {
            var providerCaps = new ProviderCapabilities
            {
                ProviderName = providerName,
                SupportedResources = resources.Select(r => new ResourceCapabilities
                {
                    ResourceName = r.Resource,
                    SupportedOperations = r.Operations?.ToList()
                }).ToList()
            };

            var mock = new Mock<IFhirProvider>(MockBehavior.Strict);
            mock.SetupGet(patient => patient.ProviderName).Returns(providerName);
            mock.SetupGet(patient => patient.Capabilities).Returns(providerCaps);
            mock.SetupGet(patient => patient.Code).Returns(GetRandomString());
            mock.SetupGet(patient => patient.Source).Returns(GetRandomString());
            mock.SetupGet(patient => patient.System).Returns(GetRandomString());
            mock.SetupGet(patient => patient.DisplayName).Returns(providerDisplayName);
            mock.SetupGet(patient => patient.FhirVersion).Returns(GetRandomString());

            mock.Setup(patient => patient.Patients.EverythingAsync(
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CreateRandomBundle());

            // If IFhirProvider has more required members, add minimal setups here.

            return mock;
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(
            Xeption expectedException)
        {
            return actualException =>
                actualException.SameExceptionAs(expectedException);
        }

        private static Expression<Func<Exception, bool>> SameExceptionAs(
            Exception expectedException)
        {
            return actualException =>
                actualException.SameExceptionAs(expectedException);
        }

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new FhirAbstractionProviderValidationException(
                    message: "Fhir abstraction provider validation errors occured, please try again",
                    innerException: innerException,
                    data: innerException.Data)
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new FhirAbstractionProviderDependencyException(
                    message: "Fhir abstraction provider dependency error occurred, please contact support.",
                    innerException: innerException,
                    data: innerException.Data),

                new FhirAbstractionProviderServiceException(
                    message: "Fhir abstraction provider service error occurred, please contact support.",
                    innerException: innerException,
                    data: innerException.Data)
            };
        }
    }
}
