// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirR4;
using System;
using System.Collections.Generic;
using System.Threading;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.R4.Abstractions;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Models.Capabilities;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Models.Resources;
using Moq;
using Patient = FhirR4::Hl7.Fhir.Model.Patient;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    public static class TestFhirProviderFactory
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
                        SupportedOperations = new List<string> { "Everything" }
                    }
                }
            };

            var providerMock = new Mock<IFhirProvider>(MockBehavior.Strict);
            var patientResourceMock = new Mock<IPatientResource>(MockBehavior.Strict);
            providerMock.SetupGet(p => p.ProviderName).Returns(providerName);
            providerMock.SetupGet(p => p.Capabilities).Returns(providerCapabilities);
            providerMock.SetupGet(p => p.System).Returns($"http://test.system/{providerName}");
            providerMock.SetupGet(p => p.Code).Returns(providerName);
            providerMock.SetupGet(p => p.Source).Returns($"Test-{providerName}");
            providerMock.SetupGet(p => p.Patients).Returns(patientResourceMock.Object);

            patientResourceMock.Setup(p => p.Everything(
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((string id, DateTimeOffset? start, DateTimeOffset? end, string typeFilter,
                    DateTimeOffset? since, int? count, CancellationToken ct) =>
                {
                    var bundle = new Bundle
                    {
                        Type = Bundle.BundleType.Searchset,
                        Meta = new Meta
                        {
                            LastUpdated = DateTimeOffset.UtcNow,
                            Source = providerName
                        },
                        Entry = new List<Bundle.EntryComponent>
                        {
                            new Bundle.EntryComponent
                            {
                                Resource = new Patient
                                {
                                    Id = id,
                                    Meta = new Meta
                                    {
                                        Source = providerName
                                    }
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
