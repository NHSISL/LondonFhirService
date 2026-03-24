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
        public async Task ShouldDeleteFhirRecordDifferenceByIdAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = await PostRandomFhirRecordDifferenceAsync();
            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference expectedFhirRecordDifference = inputFhirRecordDifference;

            // when
            FhirRecordDifference deletedFhirRecordDifference =
                await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(inputFhirRecordDifference.Id);

            List<FhirRecordDifference> actualResult =
                await this.apiBroker.GetSpecificFhirRecordDifferenceByIdAsync(inputFhirRecordDifference.Id);

            // then
            actualResult.Count().Should().Be(0);
        }
    }
}