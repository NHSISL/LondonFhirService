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
        public async Task ShouldPutFhirRecordDifferenceAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = 
                await PostRandomFhirRecordDifferenceAsync();

            FhirRecordDifference modifiedFhirRecordDifference = 
                UpdateFhirRecordDifferenceWithRandomValues(randomFhirRecordDifference);

            // when
            await this.apiBroker.PutFhirRecordDifferenceAsync(modifiedFhirRecordDifference);
            
            FhirRecordDifference actualFhirRecordDifference = await this.apiBroker
                .GetFhirRecordDifferenceByIdAsync(randomFhirRecordDifference.Id);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(modifiedFhirRecordDifference, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(actualFhirRecordDifference.Id);
        }
    }
}