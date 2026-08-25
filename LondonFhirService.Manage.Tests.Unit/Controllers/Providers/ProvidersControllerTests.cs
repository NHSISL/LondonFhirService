// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Manage.Controllers.Providers;
using Moq;
using RESTFulSense.Controllers;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Providers
{
    public partial class ProvidersControllerTests : RESTFulController
    {
        private readonly Mock<IProviderService> providerServiceMock;
        private readonly ProvidersController providersController;

        public ProvidersControllerTests()
        {
            providerServiceMock = new Mock<IProviderService>();
            providersController = new ProvidersController(providerServiceMock.Object);
        }

        public static TheoryData<Xeption> ValidationExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new ProviderServiceValidationException(
                    message: someMessage,
                    innerException: someInnerException),

                new ProviderServiceDependencyValidationException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        public static TheoryData<Xeption> ServerExceptions()
        {
            var someInnerException = new Xeption();
            string someMessage = GetRandomString();

            return new TheoryData<Xeption>
            {
                new ProviderServiceDependencyException(
                    message: someMessage,
                    innerException: someInnerException),

                new ProviderServiceException(
                    message: someMessage,
                    innerException: someInnerException)
            };
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static Provider CreateRandomProvider() =>
            CreateProviderFiller().Create();

        private static IQueryable<Provider> CreateRandomProviders()
        {
            return CreateProviderFiller()
                .Create(count: GetRandomNumber())
                    .AsQueryable();
        }

        private static Filler<Provider> CreateProviderFiller()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            string user = Guid.NewGuid().ToString();
            var filler = new Filler<Provider>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(provider => provider.CreatedBy).Use(user)
                .OnProperty(provider => provider.UpdatedBy).Use(user);

            return filler;
        }
    }
}
