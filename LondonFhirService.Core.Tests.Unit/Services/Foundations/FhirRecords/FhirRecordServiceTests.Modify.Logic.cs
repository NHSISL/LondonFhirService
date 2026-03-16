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
        public async Task ShouldModifyFhirRecordAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);
            FhirRecord randomFhirRecord = CreateRandomModifyFhirRecord(randomDateTimeOffset);
            FhirRecord inputFhirRecord = randomFhirRecord;
            FhirRecord storageFhirRecord = inputFhirRecord.DeepClone();
            storageFhirRecord.UpdatedDate = randomFhirRecord.CreatedDate;
            FhirRecord auditAppliedFhirRecord = inputFhirRecord.DeepClone();
            auditAppliedFhirRecord.UpdatedBy = randomUserId;
            auditAppliedFhirRecord.UpdatedDate = randomDateTimeOffset;
            FhirRecord auditEnsuredFhirRecord = auditAppliedFhirRecord.DeepClone();
            FhirRecord updatedFhirRecord = inputFhirRecord;
            FhirRecord expectedFhirRecord = updatedFhirRecord.DeepClone();
            Guid fhirRecordId = inputFhirRecord.Id;

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(inputFhirRecord))
                    .ReturnsAsync(auditAppliedFhirRecord);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordByIdAsync(fhirRecordId))
                    .ReturnsAsync(storageFhirRecord);

            this.securityAuditBrokerMock.Setup(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedFhirRecord, storageFhirRecord))
                    .ReturnsAsync(auditEnsuredFhirRecord);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateFhirRecordAsync(auditEnsuredFhirRecord))
                    .ReturnsAsync(updatedFhirRecord);

            // when
            FhirRecord actualFhirRecord =
                await this.fhirRecordService.ModifyFhirRecordAsync(inputFhirRecord);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(inputFhirRecord),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordByIdAsync(fhirRecordId),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedFhirRecord, storageFhirRecord),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordAsync(auditEnsuredFhirRecord),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}