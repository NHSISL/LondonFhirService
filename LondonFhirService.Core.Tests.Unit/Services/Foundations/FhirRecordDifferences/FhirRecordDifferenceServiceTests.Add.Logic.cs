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
        public async Task ShouldAddFhirRecordDifferenceAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            User randomUser = CreateRandomUser(userId: randomUserId);
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference(randomDateTimeOffset);
            FhirRecordDifference inputFhirRecordDifference = randomFhirRecordDifference;
            FhirRecordDifference auditAppliedFhirRecordDifference = inputFhirRecordDifference.DeepClone();
            auditAppliedFhirRecordDifference.CreatedBy = randomUserId;
            auditAppliedFhirRecordDifference.CreatedDate = randomDateTimeOffset;
            auditAppliedFhirRecordDifference.UpdatedBy = randomUserId;
            auditAppliedFhirRecordDifference.UpdatedDate = randomDateTimeOffset;
            FhirRecordDifference storageFhirRecordDifference = auditAppliedFhirRecordDifference.DeepClone();
            FhirRecordDifference expectedFhirRecordDifference = storageFhirRecordDifference.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(inputFhirRecordDifference))
                    .ReturnsAsync(auditAppliedFhirRecordDifference);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertFhirRecordDifferenceAsync(auditAppliedFhirRecordDifference))
                    .ReturnsAsync(storageFhirRecordDifference);

            // when
            FhirRecordDifference actualFhirRecordDifference = await this.fhirRecordDifferenceService
                .AddFhirRecordDifferenceAsync(inputFhirRecordDifference);

            // then
            actualFhirRecordDifference.Should().BeEquivalentTo(expectedFhirRecordDifference);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(inputFhirRecordDifference),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordDifferenceAsync(auditAppliedFhirRecordDifference),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}