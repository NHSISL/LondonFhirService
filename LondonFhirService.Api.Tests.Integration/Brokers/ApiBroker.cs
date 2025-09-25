// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using RESTFulSense.Clients;

namespace LondonFhirService.Api.Tests.Integration.Brokers
{
    public partial class ApiBroker
    {
        private readonly WebApplicationFactory<Program> webApplicationFactory;
        private readonly HttpClient httpClient;
        private readonly IRESTFulApiFactoryClient apiFactoryClient;

        public ApiBroker()
        {
            webApplicationFactory = new WebApplicationFactory<Program>();
            httpClient = webApplicationFactory.CreateClient();
            apiFactoryClient = new RESTFulApiFactoryClient(httpClient);
        }
    }
}