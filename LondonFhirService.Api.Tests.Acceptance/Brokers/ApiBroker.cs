// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using Attrify.InvisibleApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESTFulSense.Clients;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private readonly TestWebApplicationFactory<Program> webApplicationFactory;
        private readonly HttpClient httpClient;
        private readonly IRESTFulApiFactoryClient apiFactoryClient;
        internal readonly InvisibleApiKey invisibleApiKey;
        internal readonly IConfiguration configuration;

        internal TestWebApplicationFactory<Program> WebApplicationFactory => webApplicationFactory;

        public ApiBroker()
        {
            webApplicationFactory = new TestWebApplicationFactory<Program>();
            invisibleApiKey = webApplicationFactory.Services.GetService<InvisibleApiKey>();
            httpClient = webApplicationFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add(invisibleApiKey.Key, invisibleApiKey.Value);
            apiFactoryClient = new RESTFulApiFactoryClient(httpClient);
            configuration = webApplicationFactory.Services.GetService<IConfiguration>();
        }
    }
}