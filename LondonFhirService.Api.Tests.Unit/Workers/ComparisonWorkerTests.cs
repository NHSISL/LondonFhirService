// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Api.Workers;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Api.Tests.Unit.Workers
{
    public partial class ComparisonWorkerTests
    {
        private readonly Mock<IServiceScopeFactory> serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> serviceScopeMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IComparisonCoordinationService> comparisonCoordinationServiceMock;
        private readonly Mock<ILogger<ComparisonWorker>> loggerMock;
        private readonly TestableComparisonWorker worker;

        public ComparisonWorkerTests()
        {
            serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeMock = new Mock<IServiceScope>();
            serviceProviderMock = new Mock<IServiceProvider>();
            comparisonCoordinationServiceMock = new Mock<IComparisonCoordinationService>();
            loggerMock = new Mock<ILogger<ComparisonWorker>>();

            IOptions<ComparisonWorkerSettings> workerSettings = Options.Create(
                new ComparisonWorkerSettings { SleepIntervalSeconds = 0 });

            serviceScopeFactoryMock
                .Setup(factory => factory.CreateScope())
                .Returns(serviceScopeMock.Object);

            serviceScopeMock
                .Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            serviceProviderMock
                .Setup(provider => provider.GetService(typeof(IComparisonCoordinationService)))
                .Returns((object)comparisonCoordinationServiceMock.Object);

            worker = new TestableComparisonWorker(
                serviceScopeFactoryMock.Object,
                loggerMock.Object,
                workerSettings);
        }

        private class TestableComparisonWorker : ComparisonWorker
        {
            public TestableComparisonWorker(
                IServiceScopeFactory serviceScopeFactory,
                ILogger<ComparisonWorker> logger,
                IOptions<ComparisonWorkerSettings> settings)
                : base(serviceScopeFactory, logger, settings) { }

            public new Task ExecuteAsync(CancellationToken stoppingToken) =>
                base.ExecuteAsync(stoppingToken);
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();
    }
}
