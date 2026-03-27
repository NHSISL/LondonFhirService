// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecordDifferences;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceApiTests
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
            actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(actualFhirRecordDifference.Id);
        }
    }
}