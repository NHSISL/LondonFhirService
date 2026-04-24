// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecords
{
    public partial class FhirRecordServiceTests
    {
        [Fact]
        public async Task ShouldAddFhirRecordAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);
            FhirRecord randomFhirRecord = CreateRandomFhirRecord(randomDateTimeOffset);
            FhirRecord inputFhirRecord = randomFhirRecord;
            FhirRecord auditAppliedFhirRecord = inputFhirRecord.DeepClone();
            auditAppliedFhirRecord.CreatedBy = randomUserId;
            auditAppliedFhirRecord.CreatedDate = randomDateTimeOffset;
            auditAppliedFhirRecord.UpdatedBy = randomUserId;
            auditAppliedFhirRecord.UpdatedDate = randomDateTimeOffset;
            FhirRecord storageFhirRecord = auditAppliedFhirRecord.DeepClone();
            FhirRecord expectedFhirRecord = storageFhirRecord.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(inputFhirRecord))
                    .ReturnsAsync(auditAppliedFhirRecord);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUser.UserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertFhirRecordAsync(auditAppliedFhirRecord))
                    .ReturnsAsync(storageFhirRecord);

            // when
            FhirRecord actualFhirRecord = await this.fhirRecordService
                .AddFhirRecordAsync(inputFhirRecord);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(inputFhirRecord),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordAsync(auditAppliedFhirRecord),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}