// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Metas
{
    public partial class MetaIgnoreProcessingRuleTests
    {
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetReplacementAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            JsonElement randomElement = new();

            var expectedMetaIgnoreProcessingDependencyException =
                new MetaIgnoreProcessingDependencyException(
                    message: "Meta ignore processing dependency error occurred, contact support.",
                    innerException: dependencyException);

            var metaIgnoreProcessingRuleMock = new Mock<MetaIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            metaIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                metaIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            MetaIgnoreProcessingDependencyException actualMetaIgnoreProcessingDependencyException =
                await Assert.ThrowsAsync<MetaIgnoreProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualMetaIgnoreProcessingDependencyException
                .Should().BeEquivalentTo(expectedMetaIgnoreProcessingDependencyException);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedMetaIgnoreProcessingDependencyException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            metaIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnGetReplacementAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            JsonElement randomElement = new();

            var expectedMetaIgnoreProcessingDependencyValidationException =
                new MetaIgnoreProcessingDependencyValidationException(
                    message: "Meta ignore processing dependency validation error occurred, contact support.",
                    innerException: dependencyValidationException);

            var metaIgnoreProcessingRuleMock = new Mock<MetaIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            metaIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyValidationException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                metaIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            MetaIgnoreProcessingDependencyValidationException 
                actualMetaIgnoreProcessingDependencyValidationException =
                    await Assert.ThrowsAsync<MetaIgnoreProcessingDependencyValidationException>(
                        testCode: getReplacementTask.AsTask);

            // then
            actualMetaIgnoreProcessingDependencyValidationException
                .Should().BeEquivalentTo(expectedMetaIgnoreProcessingDependencyValidationException);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedMetaIgnoreProcessingDependencyValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            metaIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetReplacementIfResourceIsInvalidAsync()
        {
            // given
            JsonElement randomElement = new();
            var serviceException = new Exception();

            var failedMetaIgnoreProcessingException =
                new FailedMetaIgnoreProcessingException(
                    message: "Failed meta ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedMetaIgnoreProcessingServiceException =
                new MetaIgnoreProcessingServiceException(
                    message: "Meta ignore processing service error occurred, contact support.",
                    innerException: failedMetaIgnoreProcessingException);

            var metaIgnoreProcessingRuleMock = new Mock<MetaIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            metaIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    randomElement))
                        .Throws(serviceException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                metaIgnoreProcessingRuleMock.Object.GetReplacementAsync(
                    randomElement);

            MetaIgnoreProcessingServiceException actualMetaIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<MetaIgnoreProcessingServiceException>(
                    getReplacementTask.AsTask);

            // then
            actualMetaIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedMetaIgnoreProcessingServiceException);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetaIgnoreProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            metaIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}