// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        /// <summary>
        /// The raw response rather than a deserialised body. The correlation id travels in a
        /// header, which every other call here throws away along with the status code. Headers go
        /// on the request rather than on the shared HttpClient, so one test cannot leak a trace
        /// into the next.
        /// </summary>
        public async ValueTask<HttpResponseMessage> GetResponseAsync(
            string relativeUrl,
            IDictionary<string, string> headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

            if (headers is not null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return await this.httpClient.SendAsync(request);
        }
    }
}
