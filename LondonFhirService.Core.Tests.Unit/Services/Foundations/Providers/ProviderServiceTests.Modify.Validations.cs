// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
        public async Task ShouldThrowValidationExceptionOnModifyIfProviderIsNullAndLogItAsync()
        {
            // given
            Provider nullProvider = null;
            var nullProviderServiceException = new NullProviderServiceException(message: "Provider is null.");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: nullProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(nullProvider))
                    .ReturnsAsync(nullProvider);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(nullProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(nullProvider),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfProviderIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            string randomUserId = GetRandomString();

            var invalidProvider = new Provider
            {
                Name = invalidText,
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
                values: "Text is required");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.UpdatedDate),
                values:
                [
                    "Date is required",
                    $"Date is the same as {nameof(Provider.CreatedDate)}"
                ]);

            invalidProviderServiceException.AddData(
                key: nameof(Provider.UpdatedBy),
                values:
                [
                    "Text is required",
                    $"Expected value to be '{randomUserId}' but found '{invalidProvider.UpdatedBy}'."
                ]);

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: invalidProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidProvider))
                    .ReturnsAsync(invalidProvider);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(invalidProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    modifyProviderTask.AsTask);

            //then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidProvider),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once());

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateProviderAsync(It.IsAny<Provider>()),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfProviderHasInvalidLengthProperty()
        {
            // given
            string randomUserId = GetRandomString();
            var invalidProvider = CreateRandomModifyProvider(GetRandomDateTimeOffset(), userId: randomUserId);
            invalidProvider.Name = GetRandomStringWithLengthOf(501);

            var invalidProviderServiceException =
                new InvalidProviderServiceException(
                    message: "Invalid provider. Please correct the errors and try again.");


            invalidProviderServiceException.AddData(
                key: nameof(Provider.Name),
                values: $"Text exceeds max length of {invalidProvider.Name.Length - 1} characters");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: invalidProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidProvider))
                    .ReturnsAsync(invalidProvider);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(invalidProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidProvider),
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
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsSameAsCreatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();

            Provider randomProvider =
                CreateRandomProvider(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            Provider invalidProvider = randomProvider;

            var invalidProviderServiceException =
                new InvalidProviderServiceException(
                    message: "Invalid provider. Please correct the errors and try again.");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.UpdatedDate),
                values: $"Date is the same as {nameof(Provider.CreatedDate)}");

            var expectedProviderServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: invalidProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidProvider))
                    .ReturnsAsync(invalidProvider);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Provider> modifyProviderTask = this.providerService.ModifyProviderAsync(invalidProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderServiceValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(invalidProvider),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderServiceValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(invalidProvider.Id),
                    Times.Never);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MinutesBeforeOrAfter))]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsNotRecentAndLogItAsync(int minutes)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset invalidDate = randomDateTimeOffset.AddMinutes(minutes);
            DateTimeOffset startDate = randomDateTimeOffset.AddSeconds(-90);
            DateTimeOffset endDate = randomDateTimeOffset.AddSeconds(0);
            string randomUserId = GetRandomString();

            Provider randomProvider =
                CreateRandomProvider(dateTimeOffset: randomDateTimeOffset, userId: randomUserId);

            randomProvider.UpdatedDate = randomDateTimeOffset.AddMinutes(minutes);

            var invalidProviderServiceException =
                new InvalidProviderServiceException(
                    message: "Invalid provider. Please correct the errors and try again.");

            invalidProviderServiceException.AddData(
                key: nameof(Provider.UpdatedDate),
                values:
                    $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidDate}");

            var expectedProviderValidatonException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: invalidProviderServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(randomProvider))
                    .ReturnsAsync(randomProvider);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Provider> modifyProviderTask =
                this.providerService.ModifyProviderAsync(randomProvider);

            ProviderServiceValidationException actualProviderServiceValidationException =
                await Assert.ThrowsAsync<ProviderServiceValidationException>(
                    modifyProviderTask.AsTask);

            // then
            actualProviderServiceValidationException.Should()
                .BeEquivalentTo(expectedProviderValidatonException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(randomProvider),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedProviderValidatonException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectProviderByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
