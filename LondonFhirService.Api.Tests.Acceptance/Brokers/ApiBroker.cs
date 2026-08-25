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
        private readonly TestWebApplicationFactory webApplicationFactory;
        private readonly HttpClient httpClient;
        private readonly IRESTFulApiFactoryClient apiFactoryClient;
        internal readonly IConfiguration configuration;

        internal TestWebApplicationFactory WebApplicationFactory => webApplicationFactory;

        public ApiBroker()
        {
            webApplicationFactory = new TestWebApplicationFactory();
            httpClient = webApplicationFactory.CreateClient();

            // The API host no longer registers an InvisibleApiKey - the only endpoints that
            // carried [InvisibleApi] were the admin CRUD controllers, which now live on the
            // management host. Resolving and dereferencing it here threw in this constructor,
            // and because this type is the collection fixture that failed every acceptance
            // test in the project rather than only the ones targeting those endpoints.
            InvisibleApiKey invisibleApiKey =
                webApplicationFactory.Services.GetService<InvisibleApiKey>();

            if (invisibleApiKey is not null
                && string.IsNullOrWhiteSpace(invisibleApiKey.Key) == false)
            {
                httpClient.DefaultRequestHeaders.Add(invisibleApiKey.Key, invisibleApiKey.Value);
            }

            apiFactoryClient = new RESTFulApiFactoryClient(httpClient);
            configuration = webApplicationFactory.Services.GetService<IConfiguration>();
        }
    }
}