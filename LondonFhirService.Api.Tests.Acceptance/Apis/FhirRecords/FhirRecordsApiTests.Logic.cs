// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Tests.Acceptance.Models.FhirRecords;

namespace LondonFhirService.Api.Tests.Acceptance.Apis
{
    public partial class FhirRecordsApiTests
    {
        [Fact]
        public async Task ShouldPostFhirRecordAsync()
        {
            // given
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            FhirRecord inputFhirRecord = randomFhirRecord;
            FhirRecord expectedFhirRecord = inputFhirRecord;

            // when
            await this.apiBroker.PostFhirRecordAsync(inputFhirRecord);

            FhirRecord actualFhirRecord =
                await this.apiBroker.GetFhirRecordByIdAsync(inputFhirRecord.Id);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord,
                options => options
                    .Excluding(fhirRecord => fhirRecord.CreatedBy)
                    .Excluding(fhirRecord => fhirRecord.CreatedDate)
                    .Excluding(fhirRecord => fhirRecord.UpdatedBy)
                    .Excluding(fhirRecord => fhirRecord.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldGetAllFhirRecordsAsync()
        {
            // given
            List<FhirRecord> randomFhirRecords = await PostRandomFhirRecordsAsync();
            List<FhirRecord> expectedFhirRecords = randomFhirRecords;

            // when
            var actualFhirRecords = await this.apiBroker.GetAllFhirRecordsAsync();

            // then
            foreach (FhirRecord expectedFhirRecord in expectedFhirRecords)
            {
                FhirRecord actualFhirRecord = actualFhirRecords
                    .Single(fhirRecord => fhirRecord.Id == expectedFhirRecord.Id);

                actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord,
                    options => options
                        .Excluding(fhirRecord => fhirRecord.CreatedBy)
                        .Excluding(fhirRecord => fhirRecord.CreatedDate)
                        .Excluding(fhirRecord => fhirRecord.UpdatedBy)
                        .Excluding(fhirRecord => fhirRecord.UpdatedDate));

                await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
            }
        }

        [Fact]
        public async Task ShouldGetFhirRecordByIdAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord expectedFhirRecord = randomFhirRecord;

            // when
            FhirRecord actualFhirRecord =
                await this.apiBroker.GetFhirRecordByIdAsync(randomFhirRecord.Id);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord,
                options => options
                    .Excluding(fhirRecord => fhirRecord.CreatedBy)
                    .Excluding(fhirRecord => fhirRecord.CreatedDate)
                    .Excluding(fhirRecord => fhirRecord.UpdatedBy)
                    .Excluding(fhirRecord => fhirRecord.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldPutFhirRecordAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord modifiedFhirRecord = UpdateFhirRecordWithRandomValues(randomFhirRecord);

            // when
            await this.apiBroker.PutFhirRecordAsync(modifiedFhirRecord);

            FhirRecord actualFhirRecord =
                await this.apiBroker.GetFhirRecordByIdAsync(randomFhirRecord.Id);

            // then
            actualFhirRecord.Should().BeEquivalentTo(modifiedFhirRecord,
                options => options
                    .Excluding(fhirRecord => fhirRecord.CreatedBy)
                    .Excluding(fhirRecord => fhirRecord.CreatedDate)
                    .Excluding(fhirRecord => fhirRecord.UpdatedBy)
                    .Excluding(fhirRecord => fhirRecord.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
        }

        [Fact]
        public async Task ShouldDeleteFhirRecordAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord inputFhirRecord = randomFhirRecord;

            // when
            await this.apiBroker.DeleteFhirRecordByIdAsync(inputFhirRecord.Id);

            List<FhirRecord> actualResult =
                await this.apiBroker.GetSpecificFhirRecordByIdAsync(inputFhirRecord.Id);

            // then
            actualResult.Count().Should().Be(0);
        }
    }
}
