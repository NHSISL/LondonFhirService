// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecords;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.FhirRecords
{
    public partial class FhirRecordApiTests
    {
        /// <summary>
        /// The comparisons page lists what is still in the compare queue, and it cannot ask for
        /// whole rows to do it: a FhirRecord carries the provider's entire bundle in JsonPayload,
        /// so fifty unprojected rows would move a patient record apiece to render fifty short
        /// "still waiting" lines. It asks for six columns instead.
        ///
        /// A projection is served through OData's SelectExpandWrapper rather than by the host's
        /// own serialiser, which is a separate code path making its own naming choices - and the
        /// page reads the answer by field name. So this pins all three things the page depends on
        /// and none of which the controller shows: that the projection serialises into a readable
        /// object at all, that it keeps the same camelCase the unprojected shape uses, and that
        /// JsonPayload stays out of it.
        /// </summary>
        [Fact]
        public async Task ShouldProjectFhirRecordsWithoutThePayloadAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord inputFhirRecord = randomFhirRecord;

            string select = "Id,CorrelationId,SourceName,IsPrimarySource,Status,InsertedDate";

            // when
            string actualBody = await this.apiBroker.GetFhirRecordsProjectionRawAsync(
                select,
                $"Id eq {inputFhirRecord.Id}");

            // then
            using JsonDocument actualDocument = JsonDocument.Parse(actualBody);
            JsonElement actualRow = actualDocument.RootElement[0];

            // Asked for by the exact name the client reads, because a field that arrives under
            // another name is not an error on the far side - it is a row of blanks.
            ReadField(actualRow, "id").GetGuid().Should().Be(inputFhirRecord.Id);

            ReadField(actualRow, "correlationId").GetString().Should()
                .Be(inputFhirRecord.CorrelationId);

            ReadField(actualRow, "sourceName").GetString().Should()
                .Be(inputFhirRecord.SourceName);

            ReadField(actualRow, "isPrimarySource").GetBoolean().Should()
                .Be(inputFhirRecord.IsPrimarySource);

            // The whole point of the projection. A payload here is not a cosmetic problem: it is
            // the pending list quietly costing a FHIR bundle per queued record.
            actualRow.EnumerateObject()
                .Any(field => string.Equals(
                    field.Name,
                    "jsonPayload",
                    StringComparison.OrdinalIgnoreCase))
                .Should().BeFalse("the payload is what the projection exists to leave behind");

            await this.apiBroker.DeleteFhirRecordByIdAsync(inputFhirRecord.Id);
        }

        private static JsonElement ReadField(JsonElement row, string name)
        {
            if (row.TryGetProperty(name, out JsonElement field))
            {
                return field;
            }

            throw new Xunit.Sdk.XunitException(
                $"The projection carried no '{name}' field. It answered with: " +
                    $"{string.Join(", ", row.EnumerateObject().Select(each => each.Name))}");
        }
    }
}
