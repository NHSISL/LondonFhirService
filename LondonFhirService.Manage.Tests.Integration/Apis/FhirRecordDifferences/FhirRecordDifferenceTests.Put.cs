// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Integration.Models.FhirRecordDifferences;

namespace LondonFhirService.Manage.Tests.Integration.Apis.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceApiTests
    {
        [Fact]
        public async Task ShouldPutFhirRecordDifferenceAsync()
        {
            // given
            FhirRecordDifference randomFhirRecordDifference = await PostRandomFhirRecordDifferenceAsync();
            FhirRecordDifference modifiedFhirRecordDifference = UpdateFhirRecordDifferenceWithRandomValues(randomFhirRecordDifference);

            // when
            await this.apiBroker.PutFhirRecordDifferenceAsync(modifiedFhirRecordDifference);
            FhirRecordDifference actualFhirRecordDifference = await this.apiBroker.GetFhirRecordDifferenceByIdAsync(randomFhirRecordDifference.Id);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(
                modifiedFhirRecordDifference,
                options => options
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.CreatedDate)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedBy)
                    .Excluding(fhirRecordDifference => fhirRecordDifference.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordDifferenceByIdAsync(actualFhirRecordDifference.Id);
        }
    }
}
