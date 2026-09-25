// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.Metrics
{
    public partial class MetricProcessingServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnRetrieveExportsIfArgumentsInvalidAndLogItAsync(
            string invalidUserId)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            var invalidArgumentMetricProcessingException =
                new InvalidArgumentMetricProcessingException(
                    message: "Invalid metric processing arguments. " +
                        "Please correct the errors and try again.");

            invalidArgumentMetricProcessingException.AddData(
                key: "correlationId",
                values: "Id is invalid.");

            invalidArgumentMetricProcessingException.AddData(
                key: "userId",
                values: "Text is invalid.");

            invalidArgumentMetricProcessingException.AddData(
                key: "status",
                values: "Value is invalid.");

            invalidArgumentMetricProcessingException.AddData(
                key: "toDate",
                values: "Date must be the same as or after fromDate.");

            var expectedMetricProcessingValidationException =
                new MetricProcessingValidationException(
                    message: "Metric processing validation error occurred, please fix errors and try again.",
                    innerException: invalidArgumentMetricProcessingException);

            // when
            ValueTask<IQueryable<MetricExport>> retrieveExportsTask =
                this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId: Guid.Empty,
                    userId: invalidUserId,
                    status: (MetricStatus)99,
                    fromDate: randomDateTimeOffset,
                    toDate: randomDateTimeOffset.AddTicks(-1),
                    TestContext.Current.CancellationToken);

            MetricProcessingValidationException actualMetricProcessingValidationException =
                await Assert.ThrowsAsync<MetricProcessingValidationException>(
                    retrieveExportsTask.AsTask);

            // then
            actualMetricProcessingValidationException.Should()
                .BeEquivalentTo(expectedMetricProcessingValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMetricProcessingValidationException))),
                        Times.Once);

            this.metricServiceMock.Verify(service =>
                service.RetrieveAllMetricsAsync(It.IsAny<CancellationToken>()),
                    Times.Never);

            this.metricServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
