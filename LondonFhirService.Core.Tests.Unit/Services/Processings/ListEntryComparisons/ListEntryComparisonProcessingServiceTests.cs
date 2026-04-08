// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Processings.ListEntryComparisons;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ListEntryComparisons
{
    public partial class ListEntryComparisonProcessingServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ListEntryComparisonProcessingService listEntryComparisonProcessingService;

        public ListEntryComparisonProcessingServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.listEntryComparisonProcessingService =
                new ListEntryComparisonProcessingService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static JsonElement CreateListElementWithEntries(int entryCount)
        {
            string entries = string.Join(
                ",",
                Enumerable.Range(0, entryCount).Select(_ => "{}"));

            string json = $"{{\"entry\":[{entries}]}}";

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}
