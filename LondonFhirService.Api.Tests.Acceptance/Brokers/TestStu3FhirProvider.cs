// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirSTU3;
using System;
using System.Collections.Generic;
using System.Threading;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Capabilities;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Resources;
using Moq;
using Patient = FhirSTU3::Hl7.Fhir.Model.Patient;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    public static class TestStu3FhirProviderFactory
    {
        public static IFhirProvider CreateTestProvider(string providerName)
        {
            var providerCapabilities = new ProviderCapabilities
            {
                ProviderName = providerName,
                SupportedResources = new List<ResourceCapabilities>
                {
                    new ResourceCapabilities
                    {
                        ResourceName = "Patients",
                        SupportedOperations = new List<string>
                        {
                            "Everything",
                            "GetStructuredRecord"
                        }
                    }
                }
            };

            var providerMock = new Mock<IFhirProvider>(MockBehavior.Strict);
            var patientResourceMock = new Mock<IPatientResource>(MockBehavior.Strict);
            providerMock.SetupGet(provider => provider.ProviderName).Returns(providerName);
            providerMock.SetupGet(provider => provider.Capabilities).Returns(providerCapabilities);
            providerMock.SetupGet(provider => provider.System).Returns($"http://test.system/{providerName}");
            providerMock.SetupGet(provider => provider.Code).Returns(providerName);
            providerMock.SetupGet(provider => provider.Source).Returns($"Test-{providerName}");
            providerMock.SetupGet(provider => provider.FhirVersion).Returns("STU3");
            providerMock.SetupGet(provider => provider.Patients).Returns(patientResourceMock.Object);

            patientResourceMock.Setup(patientResource => patientResource.Everything(
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, DateTimeOffset? start, DateTimeOffset? end,
                        string typeFilter, DateTimeOffset? since, int? count,
                        CancellationToken cancellationToken) =>
                    {
                        var bundle = new Bundle
                        {
                            Type = Bundle.BundleType.Searchset,

                            Meta = new Meta
                            {
                                LastUpdated = DateTimeOffset.UtcNow
                            },

                            Entry = new List<Bundle.EntryComponent>
                            {
                                new Bundle.EntryComponent
                                {
                                    Resource = new Patient
                                    {
                                        Id = id
                                    }
                                }
                            }
                        };

                        return bundle;
                    });

            patientResourceMock.Setup(patientResource => patientResource.GetStructuredRecord(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string nhsNumber, DateTime dateOfBirth, bool demographicsOnly,
                        bool includeInactivePatients, CancellationToken cancellationToken) =>
                    {
                        var bundle = new Bundle
                        {
                            Type = Bundle.BundleType.Searchset,

                            Meta = new Meta
                            {
                                LastUpdated = DateTimeOffset.UtcNow
                            },

                            Entry = new List<Bundle.EntryComponent>
                            {
                                new Bundle.EntryComponent
                                {
                                    Resource = new Patient
                                    {
                                        Id = nhsNumber
                                    }
                                }
                            }
                        };

                        return bundle;
                    });

            return providerMock.Object;
        }
    }
}
