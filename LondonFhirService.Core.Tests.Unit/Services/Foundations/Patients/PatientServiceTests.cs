// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Brokers.Fhirs;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Services.Foundations.Patients;
using Moq;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        private readonly Mock<IFhirBroker> fhirBrokerMock;
        private readonly PatientServiceConfig patientServiceConfig;
        private readonly IPatientService patientService;

        public PatientServiceTests()
        {
            this.fhirBrokerMock = new Mock<IFhirBroker>();

            this.patientServiceConfig = new PatientServiceConfig
            {
                MaxProviderWaitTimeSeconds = 60
            };

            this.patientService = new PatientService(
                fhirBroker: this.fhirBrokerMock.Object,
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

            bundle.Entry = new List<Bundle.EntryComponent>{
                new Bundle.EntryComponent
                {
                    FullUrl = $"https://api.service.nhs.uk/personal-demographics/FHIR/R4/Patient/{patient.Id}",
                    Search = new Bundle.SearchComponent { Score = 1 },
                    Resource = patient
                }
            };


            return bundle;
        }
    }
}
