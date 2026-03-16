// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceServiceTests
    {
        [Fact]
        public async Task ShouldModifyFhirRecordDifferenceAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);
            FhirRecordDifference randomFhirRecordDifference = CreateRandomModifyFhirRecordDifference(randomDateTimeOffset);
            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference storageFhirRecordDifference = inputFhirRecordDifference.DeepClone();
            storageFhirRecordDifference.UpdatedDate = randomFhirRecordDifference.CreatedDate;
            FhirRecordDifference auditAppliedFhirRecordDifference = inputFhirRecordDifference.DeepClone();
            auditAppliedFhirRecordDifference.UpdatedBy = randomUserId;
            auditAppliedFhirRecordDifference.UpdatedDate = randomDateTimeOffset;
            FhirRecordDifference auditEnsuredFhirRecordDifference = auditAppliedFhirRecordDifference.DeepClone();
            FhirRecordDifference updatedFhirRecordDifference = inputFhirRecordDifference;
            FhirRecordDifference expectedFhirRecordDifference = updatedFhirRecordDifference.DeepClone();
            Guid fhirRecordDifferenceId = inputFhirRecordDifference.Id;

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(inputFhirRecordDifference))
                    .ReturnsAsync(auditAppliedFhirRecordDifference);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId))
                    .ReturnsAsync(storageFhirRecordDifference);

            this.securityAuditBrokerMock.Setup(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedFhirRecordDifference, storageFhirRecordDifference))
                    .ReturnsAsync(auditEnsuredFhirRecordDifference);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateFhirRecordDifferenceAsync(auditEnsuredFhirRecordDifference))
                    .ReturnsAsync(updatedFhirRecordDifference);

            // when
            FhirRecordDifference actualFhirRecordDifference =
                await this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(inputFhirRecordDifference);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(inputFhirRecordDifference),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedFhirRecordDifference, storageFhirRecordDifference),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateFhirRecordDifferenceAsync(auditEnsuredFhirRecordDifference),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}