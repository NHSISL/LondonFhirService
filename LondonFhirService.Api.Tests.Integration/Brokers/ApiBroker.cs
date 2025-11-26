// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using LondonFhirService.Api.Tests.Integration.Apis.Patient;
using RESTFulSense.Clients;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private readonly TestWebApplicationFactory webApplicationFactory;
        private readonly HttpClient httpClient;
        private readonly IRESTFulApiFactoryClient apiFactoryClient;

        internal TestWebApplicationFactory WebApplicationFactory => webApplicationFactory;

        public ApiBroker()
        {
            webApplicationFactory = new TestWebApplicationFactory();
            httpClient = webApplicationFactory.CreateClient();
            apiFactoryClient = new RESTFulApiFactoryClient(httpClient);
        }
    }
}