// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using Moq;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Core.Tests.Unit.Brokers.AuditAndMetrics
{
    public partial class AuditStorageBrokerTests
    {
        private readonly Mock<IStorageBrokerFactory> storageBrokerFactoryMock;
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly AuditStorageBroker auditStorageBroker;

        public AuditStorageBrokerTests()
        {
            this.storageBrokerFactoryMock = new Mock<IStorageBrokerFactory>();
            this.storageBrokerMock = new Mock<IStorageBroker>();

            // Writes take their own short lived context from the factory, which is what makes
            // them safe to dispatch to the background.
            this.storageBrokerFactoryMock.Setup(factory => factory.CreateStorageBrokerAsync())
                .ReturnsAsync(this.storageBrokerMock.Object);

            this.auditStorageBroker = new AuditStorageBroker(
                storageBrokerFactory: this.storageBrokerFactoryMock.Object,
                storageBroker: this.storageBrokerMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: DateTime.UtcNow.AddYears(-1)).GetValue();
    }
}
