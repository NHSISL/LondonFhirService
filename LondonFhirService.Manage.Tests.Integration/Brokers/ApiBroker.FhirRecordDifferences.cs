// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Integration.Models.FhirRecordDifferences;

namespace LondonFhirService.Manage.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private const string fhirRecordDifferencesRelativeUrl = "api/fhirrecorddifferences";

        public async ValueTask<FhirRecordDifference> PostFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference) =>
            await this.apiFactoryClient.PostContentAsync(fhirRecordDifferencesRelativeUrl, fhirRecordDifference);

        public async ValueTask<List<FhirRecordDifference>> GetAllFhirRecordDifferencesAsync() =>
            await this.apiFactoryClient.GetContentAsync<List<FhirRecordDifference>>(fhirRecordDifferencesRelativeUrl);

        public async ValueTask<FhirRecordDifference> GetFhirRecordDifferenceByIdAsync(Guid fhirRecordDifferenceId) =>
            await this.apiFactoryClient.GetContentAsync<FhirRecordDifference>($"{fhirRecordDifferencesRelativeUrl}/{fhirRecordDifferenceId}");

        public async ValueTask<FhirRecordDifference> PutFhirRecordDifferenceAsync(FhirRecordDifference fhirRecordDifference) =>
            await this.apiFactoryClient.PutContentAsync(fhirRecordDifferencesRelativeUrl, fhirRecordDifference);

        public async ValueTask<FhirRecordDifference> DeleteFhirRecordDifferenceByIdAsync(Guid fhirRecordDifferenceId) =>
            await this.apiFactoryClient.DeleteContentAsync<FhirRecordDifference>($"{fhirRecordDifferencesRelativeUrl}/{fhirRecordDifferenceId}");
    }
}