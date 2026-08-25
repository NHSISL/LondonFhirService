// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net.Http;
using Attrify.InvisibleApi.Models;
using Microsoft.Extensions.DependencyInjection;
using RESTFulSense.Clients;

namespace LondonFhirService.Manage.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private readonly TestWebApplicationFactory webApplicationFactory;
        private readonly HttpClient httpClient;
        private readonly IRESTFulApiFactoryClient apiFactoryClient;
        internal readonly InvisibleApiKey invisibleApiKey;

        /// <summary>
        /// A second client that deliberately omits the invisible-api key header, so a test can
        /// assert that the hidden verbs are unroutable to anyone who does not hold it. The keyed
        /// client above exists to seed and tear down; this one is what a real caller looks like.
        /// </summary>
        private readonly HttpClient keylessHttpClient;

        public ApiBroker()
        {
            webApplicationFactory = new TestWebApplicationFactory();
            invisibleApiKey = webApplicationFactory.Services.GetService<InvisibleApiKey>();
            httpClient = webApplicationFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add(invisibleApiKey.Key, invisibleApiKey.Value);
            apiFactoryClient = new RESTFulApiFactoryClient(httpClient);
            keylessHttpClient = webApplicationFactory.CreateClient();
        }
    }
}
