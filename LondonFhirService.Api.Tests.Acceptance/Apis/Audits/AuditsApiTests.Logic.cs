// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Tests.Acceptance.Models.Audits;

namespace LondonFhirService.Api.Tests.Acceptance.Apis
{
    public partial class AuditsApiTests
    {
        [Fact]
        public async Task ShouldPostAuditAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            Audit inputAudit = randomAudit;
            Audit expectedAudit = inputAudit;

            // when 
            await this.apiBroker.PostAuditAsync(inputAudit);

            Audit actualAudit =
                await this.apiBroker.GetAuditByIdAsync(inputAudit.Id);

            // then
            actualAudit.Should().BeEquivalentTo(expectedAudit);
            await this.apiBroker.DeleteAuditByIdAsync(actualAudit.Id);
        }

        [Fact]
        public async Task ShouldGetAllAuditsAsync()
        {
            // given
            List<Audit> randomAudits = await PostRandomAuditsAsync();
            List<Audit> expectedAudits = randomAudits;

            // when
            var actualAudits = await this.apiBroker.GetAllAuditsAsync();

            // then
            foreach (Audit expectedAudit in expectedAudits)
            {
                Audit actualAudit = actualAudits
                    .Single(actualAudit => actualAudit.Id == expectedAudit.Id);

                actualAudit.Should().BeEquivalentTo(expectedAudit);
                await this.apiBroker.DeleteAuditByIdAsync(actualAudit.Id);
            }
        }

        [Fact]
        public async Task ShouldGetAuditByIdAsync()
        {
            // given
            Audit randomAudit = await PostRandomAuditAsync();
            Audit expectedAudit = randomAudit;

            // when
            var actualAudit = await this.apiBroker.GetAuditByIdAsync(randomAudit.Id);

            // then
            actualAudit.Should().BeEquivalentTo(expectedAudit);
            await this.apiBroker.DeleteAuditByIdAsync(actualAudit.Id);
        }

        [Fact]
        public async Task ShouldPutAuditAsync()
        {
            // given
            Audit randomAudit = await PostRandomAuditAsync();
            Audit modifiedAudit = UpdateAuditWithRandomValues(randomAudit);

            // when
            await this.apiBroker.PutAuditAsync(modifiedAudit);
            var actualAudit = await this.apiBroker.GetAuditByIdAsync(randomAudit.Id);

            // then
            actualAudit.Should().BeEquivalentTo(modifiedAudit);
            await this.apiBroker.DeleteAuditByIdAsync(actualAudit.Id);
        }

        [Fact]
        public async Task ShouldDeleteAuditAsync()
        {
            // given
            Audit randomAudit = await PostRandomAuditAsync();
            Audit inputAudit = randomAudit;
            Audit expectedAudit = inputAudit;

            // when
            Audit deletedAudit =
                await this.apiBroker.DeleteAuditByIdAsync(inputAudit.Id);

            List<Audit> actualResult =
                await this.apiBroker.GetSpecificAuditByIdAsync(inputAudit.Id);

            // then
            actualResult.Count().Should().Be(0);
        }
    }
}
