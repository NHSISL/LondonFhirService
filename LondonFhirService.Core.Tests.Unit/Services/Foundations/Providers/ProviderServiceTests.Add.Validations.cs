// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Providers
{
    public partial class ProviderServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfProviderIsNullAndLogItAsync()
        {
            // given
            Provider nullProvider = null;

            var nullProviderServiceException =
                new NullProviderServiceException(message: "Provider is null.");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: nullProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                    broker.ApplyAddAuditValuesAsync(nullProvider))
                .ReturnsAsync(nullProvider);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(nullProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(() =>
                    addProviderTask.AsTask());

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(nullProvider),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIfProviderIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            string randomUserId = GetRandomString();

            var invalidProvider = new Provider
            {
                Name = invalidText
            };

            var invalidProviderServiceException =
                new InvalidProviderServiceException(
                    message: "Invalid provider. Please correct the errors and try again.");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.Id),
                values: "Id is required");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.Name),
                values: "Text is required");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.CreatedDate),
                values: "Date is required");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.CreatedBy),
                values:
                    [
                        "Text is required",
                        $"Expected value to be '{randomUserId}' but found '{invalidProvider.CreatedBy}'."
                    ]);

            invalidProviderServiceException.AddData(
                key: nameof(Provider.UpdatedDate),
                values: "Date is required");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.UpdatedBy),
                values: "Text is required");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: invalidProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidProvider))
                    .ReturnsAsync(invalidProvider);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(invalidProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(() =>
                    addProviderTask.AsTask());

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidProvider),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfProviderHasInvalidLengthProperty()
        {
            // given
            string randomUserId = GetRandomString();
            var invalidProvider = CreateRandomProvider(GetRandomDateTimeOffset(), userId: randomUserId);
            invalidProvider.Name = GetRandomStringWithLengthOf(501);

            var invalidProviderServiceException =
                new InvalidProviderServiceException(
                    message: "Invalid provider. Please correct the errors and try again.");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.Name),
                values: $"Text exceed max length of {invalidProvider.Name.Length - 1} characters");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: invalidProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalidProvider))
                    .ReturnsAsync(invalidProvider);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Provider> addProviderTask =
                this.providerService.AddProviderAsync(invalidProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    addProviderTask.AsTask);

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalidProvider),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
