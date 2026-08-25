// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Models.Audits;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private const string auditsRelativeUrl = "api/audits";

        public async ValueTask<Audit> PostAuditAsync(Audit audit) =>
            await this.apiFactoryClient.PostContentAsync(auditsRelativeUrl, audit);

        public async ValueTask<List<Audit>> GetAllAuditsAsync() =>
            await this.apiFactoryClient.GetContentAsync<List<Audit>>($"{auditsRelativeUrl}/");

        public async ValueTask<Audit> GetAuditByIdAsync(Guid auditId) =>
            await this.apiFactoryClient.GetContentAsync<Audit>($"{auditsRelativeUrl}/{auditId}");

        public async ValueTask<Audit> PutAuditAsync(Audit audit) =>
            await this.apiFactoryClient.PutContentAsync(auditsRelativeUrl, audit);

        public async ValueTask<Audit> DeleteAuditByIdAsync(Guid auditId) =>
            await this.apiFactoryClient.DeleteContentAsync<Audit>($"{auditsRelativeUrl}/{auditId}");

        // Keyless: what a caller without the invisible-api header actually gets.
        public async ValueTask<HttpResponseMessage> PostAuditWithoutKeyAsync(Audit audit) =>
            await this.keylessHttpClient.PostAsJsonAsync(auditsRelativeUrl, audit);

        public async ValueTask<HttpResponseMessage> PutAuditWithoutKeyAsync(Audit audit) =>
            await this.keylessHttpClient.PutAsJsonAsync(auditsRelativeUrl, audit);

        public async ValueTask<HttpResponseMessage> DeleteAuditByIdWithoutKeyAsync(Guid auditId) =>
            await this.keylessHttpClient.DeleteAsync($"{auditsRelativeUrl}/{auditId}");

        public async ValueTask<HttpResponseMessage> GetAllAuditsWithoutKeyAsync() =>
            await this.keylessHttpClient.GetAsync($"{auditsRelativeUrl}/");
    }
}
