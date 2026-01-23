// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using LondonFhirService.Api.Tests.Integration.Apis.Patient;
using RESTFulSense.Clients;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private readonly TestWebApplicationFactory webApplicationFactory;
        private readonly HttpClient httpClient;
        private readonly HttpClient ddsAuthHttpClient;
        private readonly HttpClient ddsHttpClient;
        private readonly IRESTFulApiFactoryClient apiFactoryClient;

        internal TestWebApplicationFactory WebApplicationFactory => webApplicationFactory;

        public ApiBroker()
        {
            webApplicationFactory = new TestWebApplicationFactory();
            httpClient = webApplicationFactory.CreateClient();

            this.ddsAuthHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://devauthentication.discoverydataservice.net/")
            };

            this.ddsHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://devfhirapi.discoverydataservice.net:8443")
            };

            apiFactoryClient = new RESTFulApiFactoryClient(httpClient);
        }
    }
}