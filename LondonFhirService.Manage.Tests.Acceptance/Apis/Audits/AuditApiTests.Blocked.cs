// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.Audits;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Audits
{
    /// <summary>
    /// The write verbs carry [InvisibleApi]. The middleware answers 404 rather than 401 or 403 to
    /// anyone without the key header, so the endpoint does not advertise that it exists at all.
    ///
    /// Success here is the block. These endpoints are not an operator-facing way to rewrite
    /// compliance records - they exist so this suite can seed a row and clear it again.
    /// </summary>
    public partial class AuditApiTests
    {
        [Fact]
        public async Task ShouldBlockPostAuditWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();

            // when
            var response = await this.apiBroker.PostAuditWithoutKeyAsync(randomAudit);

            // then
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ShouldBlockPutAuditWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();

            // when
            var response = await this.apiBroker.PutAuditWithoutKeyAsync(randomAudit);

            // then
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ShouldBlockDeleteAuditWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();

            // when
            var response = await this.apiBroker.DeleteAuditByIdWithoutKeyAsync(randomId);

            // then
            // A random id, deliberately. If the block were ever lifted this would answer 404 for
            // a different reason, so the seeded case below is what proves the block is real.
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ShouldBlockDeleteOfAnExistingAuditWithoutTheInvisibleApiKeyAsync()
        {
            // given
            Audit randomAudit = await PostRandomAuditAsync();

            // when
            var response = await this.apiBroker.DeleteAuditByIdWithoutKeyAsync(randomAudit.Id);

            // then
            // The row exists, so a routable endpoint would have deleted it. 404 here is the
            // middleware hiding the endpoint, not the service failing to find the row.
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            Audit stillThere = await this.apiBroker.GetAuditByIdAsync(randomAudit.Id);
            stillThere.Should().NotBeNull();

            await this.apiBroker.DeleteAuditByIdAsync(randomAudit.Id);
        }
    }
}
