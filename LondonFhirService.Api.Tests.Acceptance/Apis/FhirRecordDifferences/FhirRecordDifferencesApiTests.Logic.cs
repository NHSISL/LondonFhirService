// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Tests.Acceptance.Models.FhirRecordDifferences;

namespace LondonFhirService.Api.Tests.Acceptance.Apis.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesApiTests
    {
        [Fact]
        public async Task ShouldPostFhirRecordDifferenceAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference expectedFhirRecordDifference = inputFhirRecordDifference;

            // when
            await this.apiBroker.PostFhirRecordDifferenceAsync(inputFhirRecordDifference);

            FhirRecordDifference actualFhirRecordDifference =
                await this.apiBroker.GetFhirRecordDifferenceByIdAsync(inputFhirRecordDifference.Id);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference,
                options => options
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedDate)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(actualFhirRecordDifference.Id);
        }

        [Fact]
        public async Task ShouldGetAllFhirRecordDifferencesAsync()
        {
            // given
            List<FhirRecordDifference> randomFhirRecordDifferences =
                await PostRandomFhirRecordDifferencesAsync();

            List<FhirRecordDifference> expectedFhirRecordDifferences = randomFhirRecordDifferences;

            // when
            var actualFhirRecordDifferences =
                await this.apiBroker.GetAllFhirRecordDifferencesAsync();

            // then
            foreach (FhirRecordDifference expectedFhirRecordDifference in expectedFhirRecordDifferences)
            {
                FhirRecordDifference actualFhirRecordDifference = actualFhirRecordDifferences
                    .Single(fhirRecordDifference =>
                        fhirRecordDifference.Id == expectedFhirRecordDifference.Id);

                actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference,
                    options => options
                        .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedBy)
                        .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedDate)
                        .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedBy)
                        .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedDate));

                await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(
                    actualFhirRecordDifference.Id);
            }
        }

        [Fact]
        public async Task ShouldGetFhirRecordDifferenceByIdAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference =
                await PostRandomFhirRecordDifferenceAsync();

            FhirRecordDifference expectedFhirRecordDifference = randomFhirRecordDifference;

            // when
            FhirRecordDifference actualFhirRecordDifference =
                await this.apiBroker.GetFhirRecordDifferenceByIdAsync(randomFhirRecordDifference.Id);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference,
                options => options
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedDate)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(actualFhirRecordDifference.Id);
        }

        [Fact]
        public async Task ShouldPutFhirRecordDifferenceAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference =
                await PostRandomFhirRecordDifferenceAsync();

            FhirRecordDifference modifiedFhirRecordDifference =
                UpdateFhirRecordDifferenceWithRandomValues(randomFhirRecordDifference);

            // when
            await this.apiBroker.PutFhirRecordDifferenceAsync(modifiedFhirRecordDifference);

            FhirRecordDifference actualFhirRecordDifference =
                await this.apiBroker.GetFhirRecordDifferenceByIdAsync(randomFhirRecordDifference.Id);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(modifiedFhirRecordDifference,
                options => options
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedDate)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(actualFhirRecordDifference.Id);
        }

        [Fact]
        public async Task ShouldDeleteFhirRecordDifferenceAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference =
                await PostRandomFhirRecordDifferenceAsync();

            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;

            // when
            await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(
                inputFhirRecordDifference.Id);

            List<FhirRecordDifference> actualResult =
                await this.apiBroker.GetSpecificFhirRecordDifferenceByIdAsync(
                    inputFhirRecordDifference.Id);

            // then
            actualResult.Count().Should().Be(0);
        }
    }
}
