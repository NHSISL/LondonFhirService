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
        public async Task ShouldAddProviderAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            Provider randomProvider = CreateRandomProvider(randomDateTimeOffset);
            Provider inputProvider = randomProvider;
            Provider auditAppliedProvider = inputProvider.DeepClone();
            auditAppliedProvider.CreatedBy = randomUserId;
            auditAppliedProvider.CreatedDate = randomDateTimeOffset;
            auditAppliedProvider.UpdatedBy = randomUserId;
            auditAppliedProvider.UpdatedDate = randomDateTimeOffset;
            Provider storageProvider = auditAppliedProvider.DeepClone();
            Provider expectedProvider = storageProvider.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(inputProvider))
                    .ReturnsAsync(auditAppliedProvider);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertProviderAsync(auditAppliedProvider))
                    .ReturnsAsync(storageProvider);

            // when
            Provider actualProvider = await this.providerService.AddProviderAsync(inputProvider);

            // then
            actualProvider.Should().BeEquivalentTo(expectedProvider);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(inputProvider),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProviderAsync(auditAppliedProvider),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
