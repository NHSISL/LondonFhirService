// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.JsonElements;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Conditions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.EpisodeOfCares;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Medications;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients;
using LondonFhirService.Core.Services.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using LondonFhirService.Core.Services.Processings.ListEntryComparisons;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ComparisonTests
    {
        private readonly IResourceMatcherProcessingService resourceMatcherProcessingService;
        private readonly IListEntryComparisonProcessingService listEntryComparisonProcessingService;
        private readonly IJsonElementService jsonElementService;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IComparisonOrchestrationService comparisonOrchestrationService;

        public ComparisonTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            ILoggingBroker loggingBroker = this.loggingBrokerMock.Object;
            var jsonElementService = new JsonElementService();

            IEnumerable<IJsonIgnoreProcessingRule> ignoreRules =
                new List<IJsonIgnoreProcessingRule>
                {
                    new ArrayOrderIgnoreProcessingRule(jsonElementService, loggingBroker),
                    new GuidIgnoreProcessingRule(jsonElementService, loggingBroker),
                    new IdIgnoreProcessingRule(jsonElementService, loggingBroker),
                    new MetaIgnoreProcessingRule(jsonElementService, loggingBroker)
                };

            IEnumerable<IResourceMatcherService> matchers =
                new List<IResourceMatcherService>
                {
                    new AllergyIntoleranceMatcherService(loggingBroker),
                    new ConditionMatcherService(loggingBroker),
                    new EpisodeOfCareMatcherService(loggingBroker),
                    new ListMatcherService(loggingBroker),
                    new MedicationMatcherService(loggingBroker),
                    new MedicationStatementMatcherService(loggingBroker),
                    new PatientMatcherService(loggingBroker)
                };

            IResourceMatcherProcessingService resourceMatcherProcessingService =
                new ResourceMatcherProcessingService(
                    matchers: matchers,
                    loggingBroker: loggingBroker);

            IListEntryComparisonProcessingService listEntryComparisonProcessingService =
                new ListEntryComparisonProcessingService(loggingBroker: loggingBroker);

            this.comparisonOrchestrationService = new ComparisonOrchestrationService(
                ignoreRules: ignoreRules,
                resourceMatcherProcessingService: resourceMatcherProcessingService,
                listEntryComparisonProcessingService: listEntryComparisonProcessingService,
                jsonElementService: jsonElementService,
                loggingBroker: loggingBroker);
        }

        private User CreateRandomUser(string userId = "")
        {
            userId = string.IsNullOrWhiteSpace(userId) ? GetRandomStringWithLengthOf(255) : userId;

            return new User(
                userId: userId,
                givenName: GetRandomString(),
                surname: GetRandomString(),
                displayName: GetRandomString(),
                email: GetRandomString(),
                jobTitle: GetRandomString(),
                roles: new List<string> { GetRandomString() },

                claims: new List<System.Security.Claims.Claim>
                {
                    new(type: GetRandomString(), value: GetRandomString())
                });
        }

        private static IQueryable<Consumer> CreateRandomConsumers()
        {
            return CreateConsumerFiller(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                .AsQueryable();
        }

        private static Consumer CreateRandomModifyConsumer(DateTimeOffset dateTimeOffset, string userId = "")
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            Consumer randomConsumer = CreateRandomConsumer(dateTimeOffset, userId);
            randomConsumer.CreatedDate = randomConsumer.CreatedDate.AddDays(randomDaysInPast);

            return randomConsumer;
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        public static TheoryData<int> MinutesBeforeOrAfter()
        {
            int randomNumber = GetRandomNumber();
            int randomNegativeNumber = GetRandomNegativeNumber();

            return new TheoryData<int>
            {
                randomNumber,
                randomNegativeNumber
            };
        }

        private static SqlException GetSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Consumer CreateRandomConsumer() =>
            CreateConsumerFiller(dateTimeOffset: GetRandomDateTimeOffset()).Create();

        private static Consumer CreateRandomConsumer(DateTimeOffset dateTimeOffset, string userId = "") =>
            CreateConsumerFiller(dateTimeOffset, userId).Create();

        private static Filler<Consumer> CreateConsumerFiller(DateTimeOffset dateTimeOffset, string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId;
            var filler = new Filler<Consumer>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(consumer => consumer.Name).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(consumer => consumer.CreatedBy).Use(userId)
                .OnProperty(consumer => consumer.UpdatedBy).Use(userId)
                .OnProperty(consumer => consumer.ConsumerAccesses).IgnoreIt();

            return filler;
        }
    }
}
