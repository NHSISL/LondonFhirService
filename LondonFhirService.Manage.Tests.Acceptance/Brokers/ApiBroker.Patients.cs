// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Models.Patients;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private const string patientsRelativeUrl = "api/patient";

        /// <summary>
        /// Returns the raw HttpResponseMessage rather than a deserialised payload. The endpoint
        /// answers a JSON string on success and a problem body on failure, and several of these
        /// tests are about which status code came back, so the response is left intact for the
        /// test to read.
        /// </summary>
        public async ValueTask<HttpResponseMessage> PostGetStructuredRecordAsync(
            StructuredRecordRequest structuredRecordRequest) =>
            await this.httpClient.PostAsJsonAsync(
                $"{patientsRelativeUrl}/getstructuredrecord",
                structuredRecordRequest);
    }
}
