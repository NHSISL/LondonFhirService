// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.FhirReconciliations.STU3
{
    public partial class Stu3FhirReconciliationServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IStu3FhirReconciliationService fhirReconciliationService;

        public Stu3FhirReconciliationServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.fhirReconciliationService =
                new Stu3FhirReconciliationService(loggingBroker: this.loggingBrokerMock.Object);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Bundle CreateRandomBundle() =>
            new Bundle
            {
                Id = GetRandomString(),
                Type = Bundle.BundleType.Searchset,
                Total = GetRandomNumber()
            };

        private static string SerializeBundle(Bundle bundle) =>
            new FhirJsonSerializer().SerializeToString(bundle);

        private static List<(string Provider, string Json)> CreateRandomBundles()
        {
            var items = new List<(string Provider, string Json)>();

            for (int index = 0; index < GetRandomNumber(); index++)
            {
                items.Add((GetRandomString(), SerializeBundle(CreateRandomBundle())));
            }

            return items;
        }

        private static Provider CreateRandomProvider()
        {
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            var filler = new Filler<Provider>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(randomDateTimeOffset)
                .OnType<DateTimeOffset?>().Use(randomDateTimeOffset);

            return filler.Create();
        }
    }
}
