// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Capabilities;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Resources;
using Moq;
using FhirJsonSerializer = Hl7.Fhir.Serialization.FhirJsonSerializer;
using Patient = Hl7.Fhir.Model.Patient;

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
                        SupportedOperations = new List<string> { "EverythingAsync", "EverythingSerialisedAsync", "GetStructuredRecordAsync", "GetStructuredRecordSerialisedAsync" }
                    }
                }
            };

            var providerSource = $"Test-{providerName}";

            var providerMock = new Mock<IFhirProvider>(MockBehavior.Strict);
            var patientResourceMock = new Mock<IPatientResource>(MockBehavior.Strict);
            providerMock.SetupGet(p => p.ProviderName).Returns(providerName);
            providerMock.SetupGet(p => p.Capabilities).Returns(providerCapabilities);
            providerMock.SetupGet(p => p.System).Returns($"http://test.system/{providerName}");
            providerMock.SetupGet(p => p.Code).Returns(providerName);
            providerMock.SetupGet(p => p.Source).Returns(providerSource);
            providerMock.SetupGet(p => p.Patients).Returns(patientResourceMock.Object);
            providerMock.SetupGet(p => p.FhirVersion).Returns("STU3");
            providerMock.SetupGet(p => p.DisplayName).Returns(providerName);

            patientResourceMock.Setup(p => p.EverythingAsync(
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
                        Type = Bundle.BundleType.Collection,
                        Meta = new Meta
                        {
                            LastUpdated = DateTimeOffset.UtcNow,
                        },
                        Entry = new List<Bundle.EntryComponent>
                        {
                            new Bundle.EntryComponent
                            {
                                Resource = new Patient
                                {
                                    Id = id,
                                }
                            }
                        }
                    };

                    return bundle;
                });

            patientResourceMock.Setup(p => p.EverythingSerialisedAsync(
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
                        Type = Bundle.BundleType.Collection,
                        Meta = new Meta
                        {
                            LastUpdated = DateTimeOffset.UtcNow,
                        },
                        Entry = new List<Bundle.EntryComponent>
                        {
                            new Bundle.EntryComponent
                            {
                                Resource = new Patient
                                {
                                    Id = id,
                                }
                            }
                        }
                    };

                    var fhirJsonSerializer = new FhirJsonSerializer();
                    var json = fhirJsonSerializer.SerializeToString(bundle);

                    return json;
                });

            patientResourceMock.Setup(p => p.GetStructuredRecordAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((
                    string nhsNumber,
                    DateTime? dateOfBirth,
                    bool? demographicsOnly,
                    bool? includeInactivePatients,
                    CancellationToken cancellationToken) =>
                {
                    var bundle = new Bundle
                    {
                        Type = Bundle.BundleType.Collection,
                        Meta = new Meta
                        {
                            LastUpdated = DateTimeOffset.UtcNow,
                        },
                        Entry = new List<Bundle.EntryComponent>
                        {
                            new Bundle.EntryComponent
                            {
                                Resource = new Patient
                                {
                                    Id = nhsNumber,
                                }
                            }
                        }
                    };

                    return bundle;
                });

            patientResourceMock.Setup(p => p.GetStructuredRecordSerialisedAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((
                    string nhsNumber,
                    DateTime? dateOfBirth,
                    bool? demographicsOnly,
                    bool? includeInactivePatients,
                    CancellationToken cancellationToken) =>
                {
                    var bundle = new Bundle
                    {
                        Type = Bundle.BundleType.Collection,
                        Meta = new Meta
                        {
                            LastUpdated = DateTimeOffset.UtcNow,
                        },
                        Entry = new List<Bundle.EntryComponent>
                        {
                            new Bundle.EntryComponent
                            {
                                Resource = new Patient
                                {
                                    Id = nhsNumber,
                                }
                            }
                        }
                    };

                    var fhirJsonSerializer = new FhirJsonSerializer();
                    var json = fhirJsonSerializer.SerializeToString(bundle);

                    return json;
                });

            return providerMock.Object;
        }
    }
}
