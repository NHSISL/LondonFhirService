// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.Audits;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Audits
{
    public partial class AuditApiTests
    {
        [Fact]
        public async Task ShouldGetAllAuditsAsync()
        {
            // given
            List<Audit> randomAudits = await PostRandomAuditsAsync();
            List<Audit> expectedAudits = randomAudits;

            // when
            List<Audit> actualAudits = await this.apiBroker.GetAllAuditsAsync();

            // then
            foreach (Audit expectedAudit in expectedAudits)
            {
                Audit actualAudit = actualAudits.Single(audit => audit.Id == expectedAudit.Id);

                actualAudit.Should().BeEquivalentTo(expectedAudit, options => options
                    .Excluding(property => property.CreatedBy)
                    .Excluding(property => property.CreatedDate)
                    .Excluding(property => property.UpdatedBy)
                    .Excluding(property => property.UpdatedDate));

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
            Audit actualAudit = await this.apiBroker.GetAuditByIdAsync(randomAudit.Id);

            // then
            actualAudit.Should().BeEquivalentTo(expectedAudit, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));

            await this.apiBroker.DeleteAuditByIdAsync(actualAudit.Id);
        }

        [Fact]
        public async Task ShouldGetAllAuditsWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Audit randomAudit = await PostRandomAuditAsync();

            // when
            var response = await this.apiBroker.GetAllAuditsWithoutKeyAsync();

            // then
            // Reads are not hidden - only create, update and delete are. A caller with the right
            // role and no key header still gets the list.
            response.IsSuccessStatusCode.Should().BeTrue();

            await this.apiBroker.DeleteAuditByIdAsync(randomAudit.Id);
        }
    }
}
