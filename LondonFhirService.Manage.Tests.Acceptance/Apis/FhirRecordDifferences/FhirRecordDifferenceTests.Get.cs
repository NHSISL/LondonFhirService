// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecordDifferences;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceApiTests
    {
        [Fact]
        public async Task ShouldGetAllFhirRecordDifferencesAsync()
        {
            // given
            List<FhirRecordDifference> randomFhirRecordDifferences = await PostRandomFhirRecordDifferencesAsync();
            List<FhirRecordDifference> expectedFhirRecordDifferences = randomFhirRecordDifferences;

            // when
            List<FhirRecordDifference> actualFhirRecordDifferences = await this.apiBroker.GetAllFhirRecordDifferencesAsync();

            // then
            foreach (FhirRecordDifference expectedFhirRecordDifference in expectedFhirRecordDifferences)
            {
                FhirRecordDifference actualFhirRecordDifference =
                    actualFhirRecordDifferences.Single(approval => approval.Id == expectedFhirRecordDifference.Id);

                actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference, options => options
                    .Excluding(property => property.CreatedBy)
                    .Excluding(property => property.CreatedDate)
                    .Excluding(property => property.UpdatedBy)
                    .Excluding(property => property.UpdatedDate));

                await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(actualFhirRecordDifference.Id);
            }
        }
    }
}