// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Fhirs;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Services.Foundations.Patients;
using LondonFhirService.Providers.FHIR.R4.Abstractions;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Models.Capabilities;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Models.Exceptions;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        private readonly Mock<IFhirAbstractionProvider> fhirAbstractionProviderMock;
        private readonly Mock<IFhirProvider> ddsFhirProviderMock;
        private readonly Mock<IFhirProvider> ldsFhirProviderMock;
        private readonly Mock<IFhirProvider> unsupportedFhirProviderMock;
        private readonly Mock<IFhirProvider> unsupportedErrorFhirProviderMock;
        private readonly FhirBroker fhirBroker;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly PatientServiceConfig patientServiceConfig;
        private readonly PatientService patientService;

        public PatientServiceTests()
        {
            this.fhirAbstractionProviderMock = new Mock<IFhirAbstractionProvider>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.patientServiceConfig = new PatientServiceConfig
            {
                MaxProviderWaitTimeMilliseconds = 3000
            };

            this.ddsFhirProviderMock = MakeProvider("DDS", ("Patients", new[] { "Everything" }));
            this.ldsFhirProviderMock = MakeProvider("LDS", ("Patients", new[] { "Everything" }));
            this.unsupportedFhirProviderMock = MakeProvider("Unsupported", ("Patients", new[] { "Read" }));
            this.unsupportedErrorFhirProviderMock = MakeProvider("UnsupportedError", ("Patients", null));

            var fhirProviders = new List<IFhirProvider>
            {
                ddsFhirProviderMock.Object,
                ldsFhirProviderMock.Object,
                unsupportedFhirProviderMock.Object,
                unsupportedErrorFhirProviderMock.Object
            };

            this.fhirAbstractionProviderMock.SetupGet(provider => provider.FhirProviders)
                .Returns(fhirProviders);

            this.fhirBroker = new FhirBroker(fhirAbstractionProviderMock.Object);

            this.patientService = new PatientService(
                fhirBroker: this.fhirBroker,
                loggingBroker: this.loggingBrokerMock.Object,
                patientServiceConfig: this.patientServiceConfig);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static Patient CreateRandomPatient()
        {
            var patient = new Patient();

            var nameFiller = new Filler<HumanName>();
            nameFiller.Setup()
                .OnProperty(n => n.Children).IgnoreIt()
                .OnProperty(n => n.Extension).IgnoreIt()
                .OnProperty(n => n.FamilyElement).IgnoreIt()
                .OnProperty(n => n.GivenElement).IgnoreIt()
                .OnProperty(n => n.NamedChildren).IgnoreIt()
                .OnProperty(n => n.Period).IgnoreIt()
                .OnProperty(n => n.PrefixElement).IgnoreIt()
                .OnProperty(n => n.SuffixElement).IgnoreIt()
                .OnProperty(n => n.TextElement).IgnoreIt()
                .OnProperty(n => n.UseElement).IgnoreIt();

            patient.Name = new List<HumanName> { nameFiller.Create() };
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
                    FullUrl = $"https://api.service.nhs.uk/personal-demographics/FHIR/R4/Patient/{patient.Id}",
                    Search = new Bundle.SearchComponent { Score = 1 },
                    Resource = patient
                }
            };


            return bundle;
        }

        private Mock<IFhirProvider> MakeProvider(
            string name,
            params (string Resource, string[] Operations)[] resources)
        {
            var providerCaps = new ProviderCapabilities
            {
                ProviderName = name,
                SupportedResources = resources.Select(r => new ResourceCapabilities
                {
                    ResourceName = r.Resource,
                    SupportedOperations = r.Operations?.ToList()
                }).ToList()
            };

            var mock = new Mock<IFhirProvider>(MockBehavior.Strict);
            mock.SetupGet(p => p.ProviderName).Returns(name);
            mock.SetupGet(p => p.Capabilities).Returns(providerCaps);

            mock.Setup(p => p.Patients.Everything(
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
