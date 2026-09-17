// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Models.Patients;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private const string patientsRelativeUrl = "api/patients";

        /// <summary>
        /// Returns the raw HttpResponseMessage rather than a deserialised payload. The endpoint
        /// answers a JSON string on success and a problem body on failure, and several of these
        /// tests are about which status code came back, so the response is left intact for the
        /// test to read.
        ///
        /// The Accept header is the one axios sends, and sending it is the point. This used to
        /// send none at all, so the content type these tests asserted was the one MVC falls back
        /// to rather than the one a browser is answered with - the same answer, but arrived at a
        /// different way, which is not a contract worth pinning.
        ///
        /// Both roads lead to text/plain, and the reason is the wildcard. MvcOptions
        /// .RespectBrowserAcceptHeader is false by default, so an Accept header containing */* is
        /// disregarded entirely and MVC takes the first formatter that can write the type -
        /// StringOutputFormatter, which answers text/plain verbatim. Measured, not assumed: drop
        /// */* from the three values below and the same request comes back application/json with
        /// the payload quoted and escaped as a JSON string.
        /// </summary>
        public async ValueTask<HttpResponseMessage> PostGetStructuredRecordAsync(
            StructuredRecordRequest structuredRecordRequest)
        {
            using var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                $"{patientsRelativeUrl}/getstructuredrecord")
            {
                Content = JsonContent.Create(structuredRecordRequest)
            };

            httpRequestMessage.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            httpRequestMessage.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/plain"));

            httpRequestMessage.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("*/*"));

            return await this.httpClient.SendAsync(httpRequestMessage);
        }
    }
}
