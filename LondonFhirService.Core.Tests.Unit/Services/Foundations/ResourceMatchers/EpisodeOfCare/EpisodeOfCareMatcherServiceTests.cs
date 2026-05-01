// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.EpisodeOfCares;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.EpisodesOfCare
{
    public partial class EpisodeOfCareMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly EpisodeOfCareMatcherService episodeOfCareMatcherService;

        public EpisodeOfCareMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.episodeOfCareMatcherService =
                new EpisodeOfCareMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new Dictionary<string, JsonElement>();

        private static JsonElement CreateEpisodeOfCareResource(
            string snomedCode,
            string onsetDateTime,
            string episodeOfCareId = "episode-1")
        {
            string json = $$"""
          {
            "resourceType": "EpisodeOfCare",
            "id": "{{episodeOfCareId}}",
            "code": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            },
            "onsetDateTime": "{{onsetDateTime}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonSnomedEpisodeOfCareResource(string onsetDateTime)
        {
            string json = $$"""
          {
            "resourceType": "EpisodeOfCare",
            "id": "episode-1",
            "code": {
              "coding": [
                {
                  "system": "http://example.org/system",
                  "code": "123456"
                }
              ]
            },
            "onsetDateTime": "{{onsetDateTime}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateResourceWithoutOnsetDateTime(string snomedCode)
        {
            string json = $$"""
          {
            "resourceType": "EpisodeOfCare",
            "id": "episode-1",
            "code": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            }
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMalformedCodingResource()
        {
            string json = """
          {
            "resourceType": "EpisodeOfCare",
            "id": "episode-1",
            "code": {
              "coding": {
                "system": "http://snomed.info/sct",
                "code": "91936005"
              }
            },
            "onsetDateTime": "2024-01-01"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateEpisodeOfCareResourceWithPeriodStart(
            string periodStart,
            string episodeOfCareId = "episode-1")
        {
            string json = $$"""
          {
            "resourceType": "EpisodeOfCare",
            "id": "{{episodeOfCareId}}",
            "period": {
              "start": "{{periodStart}}"
            }
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateEpisodeOfCareResourceWithPeriodButNoStart(
            string episodeOfCareId = "episode-1")
        {
            string json = $$"""
          {
            "resourceType": "EpisodeOfCare",
            "id": "{{episodeOfCareId}}",
            "period": {
              "end": "2024-12-31"
            }
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateEpisodeOfCareResourceWithNoPeriod(string episodeOfCareId = "episode-1")
        {
            string json = $$"""
          {
            "resourceType": "EpisodeOfCare",
            "id": "{{episodeOfCareId}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveEpisodeOfCareResource(
            string periodStart,
            string episodeOfCareId)
        {
            string json = $$"""
              {
                "resourceType": "EpisodeOfCare",
                "id": "{{episodeOfCareId}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:25:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-EpisodeOfCare-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Active episode for diabetes.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/episode-of-care-id",
                    "value": "EOC-1234c518"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "EOC-1234c518"
                  }
                ],
                "status": "active",
                "statusHistory": [
                  {
                    "status": "planned",
                    "period": {
                      "start": "2022-04-01",
                      "end": "2022-04-12"
                    }
                  },
                  {
                    "status": "active",
                    "period": {
                      "start": "{{periodStart}}"
                    }
                  }
                ],
                "type": [
                  {
                    "coding": [
                      {
                        "system": "http://hl7.org/fhir/v3/episodeofcare-type",
                        "code": "hacc",
                        "display": "Home and Community Care"
                      }
                    ]
                  }
                ],
                "diagnosis": [
                  {
                    "condition": {
                      "reference": "Condition/c0a91a7d-8a49-4d20-93b1-3e6a0d29c0a1"
                    },
                    "role": {
                      "coding": [
                        {
                          "system": "http://hl7.org/fhir/diagnosis-role",
                          "code": "CC",
                          "display": "Chief complaint"
                        }
                      ]
                    },
                    "rank": 1
                  }
                ],
                "patient": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "managingOrganization": {
                  "reference": "Organization/56000299-bbd7-4dfa-ad64-c2d8692ae20c"
                },
                "period": {
                  "start": "{{periodStart}}"
                },
                "referralRequest": [
                  {
                    "reference": "ReferralRequest/8f47d255-5e8d-4d3f-a4b1-d8c9b3e72211"
                  }
                ],
                "careManager": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
                "team": [
                  {
                    "reference": "Organization/56000299-bbd7-4dfa-ad64-c2d8692ae20c"
                  }
                ]
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}