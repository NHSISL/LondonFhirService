// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        [Fact]
        public async Task ShouldModifyProviderAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            Provider randomProvider = CreateRandomModifyProvider(randomDateTimeOffset);
            Provider inputProvider = randomProvider;
            Provider storageProvider = inputProvider.DeepClone();
            storageProvider.UpdatedDate = randomProvider.CreatedDate;
            Provider auditAppliedProvider = inputProvider.DeepClone();
            auditAppliedProvider.UpdatedBy = randomUserId;
            auditAppliedProvider.UpdatedDate = randomDateTimeOffset;
            Provider auditEnsuredProvider = auditAppliedProvider.DeepClone();
            Provider updatedProvider = inputProvider;
            Provider expectedProvider = updatedProvider.DeepClone();
            Guid providerId = inputProvider.Id;

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(inputProvider))
                    .ReturnsAsync(auditAppliedProvider);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectProviderByIdAsync(providerId))
                    .ReturnsAsync(storageProvider);

            this.securityAuditBrokerMock.Setup(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedProvider, storageProvider))
                    .ReturnsAsync(auditEnsuredProvider);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateProviderAsync(auditEnsuredProvider))
                    .ReturnsAsync(updatedProvider);

            // when
            Provider actualProvider =
                await this.providerService.ModifyProviderAsync(inputProvider);

            // then
            actualProvider.Should().BeEquivalentTo(expectedProvider);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(inputProvider),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(providerId),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker => broker
                .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(auditAppliedProvider, storageProvider),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProviderAsync(auditEnsuredProvider),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
