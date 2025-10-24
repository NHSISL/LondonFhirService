// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ISL.Security.Client.Models.Foundations.Users;
using KellermanSoftware.CompareNetObjects;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessesTests
    {
        private readonly Mock<IStorageBroker> storageBroker;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ISecurityAuditBroker> securityAuditBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ConsumerAccessService consumerAccessService;
        private readonly ICompareLogic compareLogic;

        public ConsumerAccessesTests()
        {
            this.storageBroker = new Mock<IStorageBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.securityAuditBrokerMock = new Mock<ISecurityAuditBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.compareLogic = new CompareLogic();

            this.consumerAccessService = new ConsumerAccessService(
                storageBroker: storageBroker.Object,
                dateTimeBroker: dateTimeBrokerMock.Object,
                securityAuditBroker: securityAuditBrokerMock.Object,
                loggingBroker: loggingBrokerMock.Object);
        }

        private Expression<Func<ConsumerAccess, bool>> SameConsumerAccessAs(ConsumerAccess expectedConsumerAccess)
        {
            return actualConsumerAccess =>
                this.compareLogic.Compare(expectedConsumerAccess, actualConsumerAccess)
                    .AreEqual;
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static ConsumerAccess CreateRandomConsumerAccess() =>
            CreateRandomConsumerAccess(dateTimeOffset: GetRandomDateTimeOffset(), userId: GetRandomString());

        private static ConsumerAccess CreateRandomConsumerAccess(DateTimeOffset dateTimeOffset, string userId) =>
            CreateConsumerAccessesFiller(dateTimeOffset, userId).Create();

        private static List<ConsumerAccess> CreateRandomConsumerAccesses()
        {
            return CreateConsumerAccessesFiller(dateTimeOffset: GetRandomDateTimeOffset(), userId: GetRandomString())
                .Create(GetRandomNumber())
                .ToList();
        }

        private static List<ConsumerAccess> CreateConsumerAccesses(int count = 0)
        {
            if (count == 0)
            {
                count = GetRandomNumber();
            }

            return Enumerable.Range(start: 1, count)
                .Select(number => CreateRandomConsumerAccess()).ToList();
        }

        private static DateTime? RandomReturnOfDateTimeOffsetOrNull(DateTimeOffset randomDateTimeOffset)
        {
            Random random = new Random();
            bool returnNull = random.Next(2) == 0;

            if (returnNull)
            {
                return null;
            }
            else
            {
                return DateTime.Now.AddDays(random.Next(30));
            }
        }

        private static List<OdsData> CreateRandomOdsDatasByOrgCode(
            string orgCode,
            DateTimeOffset dateTimeOffset,
            int childrenCount)
        {
            OdsData futureOdsData = CreateRandomOdsData(GetRandomString());
            futureOdsData.RelationshipWithParentStartDate = dateTimeOffset.AddDays(GetRandomNumber());
            futureOdsData.RelationshipWithParentEndDate = dateTimeOffset.AddDays(GetRandomNumber());
            OdsData pastOdsData = CreateRandomOdsData(GetRandomString());
            pastOdsData.RelationshipWithParentStartDate = dateTimeOffset.AddDays(-GetRandomNumber());
            pastOdsData.RelationshipWithParentEndDate = dateTimeOffset.AddDays(-1);
            OdsData currentActiveOdsData = CreateRandomOdsData(orgCode);
            currentActiveOdsData.RelationshipWithParentStartDate = dateTimeOffset.AddDays(-GetRandomNumber());

            currentActiveOdsData.RelationshipWithParentEndDate =
                RandomReturnOfDateTimeOffsetOrNull(dateTimeOffset.AddDays(GetRandomNumber()));

            List<OdsData> parentOdsDatas = new List<OdsData> {
                futureOdsData,
                pastOdsData,
                currentActiveOdsData,
            };

            List<OdsData> allOdsDatas = new List<OdsData>();
            allOdsDatas.AddRange(parentOdsDatas);

            for (var index = 0; index < parentOdsDatas.Count; index++)
            {
                parentOdsDatas[index].OdsHierarchy = HierarchyId.Parse($"/{index}/");

                List<OdsData> childOdsDatas = CreateRandomOdsDataChildren(
                    parentHierarchyId: parentOdsDatas[index].OdsHierarchy,
                    dateTimeOffset,
                    count: childrenCount);

                allOdsDatas.AddRange(childOdsDatas);
            }

            return allOdsDatas;
        }

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();
            result = result.Length > length ? result.Substring(0, length) : result;

            return result;
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static string GetRandomStringWithLength(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(max: 10, min: 2).GetValue();

        private SqlException CreateSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(type: typeof(SqlException));

        private static ConsumerAccess CreateRandomModifyConsumerAccess(DateTimeOffset dateTimeOffset, string userId)
        {
            int randomDaysInThePast = GetRandomNegativeNumber();
            ConsumerAccess randomConsumerAccess = CreateRandomConsumerAccess(dateTimeOffset, userId);
            randomConsumerAccess.CreatedDate = dateTimeOffset.AddDays(randomDaysInThePast);

            return randomConsumerAccess;
        }

        private User CreateRandomInvalidUser(string entraUserId)
        {
            return new User(
                userId: entraUserId,
                givenName: GetRandomString(),
                surname: GetRandomString(),
                displayName: GetRandomString(),
                email: GetRandomString(),
                jobTitle: GetRandomString(),
                roles: new List<string> { GetRandomString() },

                claims: new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(type: GetRandomString(), value: GetRandomString())
                });
        }

        private User CreateRandomUser(string userId = "")
        {
            var internalUserId = string.IsNullOrWhiteSpace(userId) ? GetRandomStringWithLengthOf(255) : userId;

            return new User(
                userId: internalUserId,
                givenName: GetRandomString(),
                surname: GetRandomString(),
                displayName: GetRandomString(),
                email: GetRandomString(),
                jobTitle: GetRandomString(),
                roles: new List<string> { GetRandomString() },

                claims: new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(type: GetRandomString(), value: GetRandomString())
                });
        }

        private static Filler<User> CreateRandomUserFiller()
        {
            var filler = new Filler<User>();
            filler.Setup();

            return filler;
        }

        private static Filler<ConsumerAccess> CreateConsumerAccessesFiller(DateTimeOffset dateTimeOffset, string userId)
        {
            var filler = new Filler<ConsumerAccess>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(consumerAccess => consumerAccess.CreatedBy).Use(userId)
                .OnProperty(consumerAccess => consumerAccess.UpdatedBy).Use(userId);

            return filler;
        }

        private static List<OdsData> CreateRandomOdsDataChildren(
            HierarchyId parentHierarchyId,
            DateTimeOffset randomDateTimeOffset,
            int count = 0)
        {
            if (parentHierarchyId == null)
            {
                parentHierarchyId = HierarchyId.Parse("/");
            }

            if (count == 0)
            {
                count = GetRandomNumber();
            }

            List<OdsData> children = CreateOdsDataFiller(organisationCode: GetRandomString())
                .Create(count)
                    .ToList();

            HierarchyId lastChildHierarchy = null;

            foreach (var child in children)
            {
                child.OdsHierarchy = parentHierarchyId.GetDescendant(lastChildHierarchy, null);
                lastChildHierarchy = child.OdsHierarchy;
                child.RelationshipWithParentStartDate = randomDateTimeOffset.AddDays(-GetRandomNumber());
                child.RelationshipWithParentEndDate = randomDateTimeOffset.AddDays(GetRandomNumber());
            }

            return children;
        }

        private static List<OdsData> CreateRandomOdsDatas(List<string> organisationCodes)
        {
            List<OdsData> odsDatas = new List<OdsData>();

            foreach (var organisationCode in organisationCodes)
            {
                odsDatas.Add(CreateRandomOdsData(organisationCode));
            }

            return odsDatas;
        }

        private static OdsData CreateRandomOdsData(string organisationCode) =>
            CreateOdsDataFiller(organisationCode).Create();

        private static Filler<OdsData> CreateOdsDataFiller(
            string organisationCode, HierarchyId hierarchyId = null)
        {
            string user = Guid.NewGuid().ToString();
            DateTimeOffset dateTimeOffset = GetRandomDateTimeOffset();
            var filler = new Filler<OdsData>();

            if (hierarchyId == null)
            {
                hierarchyId = HierarchyId.Parse("/");
            }

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use((DateTimeOffset?)default)
                .OnProperty(odsData => odsData.OrganisationCode).Use(organisationCode)
                .OnProperty(odsData => odsData.OrganisationName).Use(GetRandomStringWithLengthOf(30))
                .OnProperty(odsData => odsData.OdsHierarchy).Use(hierarchyId);

            return filler;
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(
            Xeption expectedException)
        {
            return actualException =>
                actualException.SameExceptionAs(expectedException);
        }

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
                .OnProperty(consumer => consumer.Name).Use(GetRandomStringWithLengthOf(36))
                .OnProperty(consumer => consumer.CreatedBy).Use(userId)
                .OnProperty(consumer => consumer.UpdatedBy).Use(userId)
                .OnProperty(consumer => consumer.ConsumerAccesses).IgnoreIt();

            return filler;
        }
    }
}