// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Moq;
using Xeptions;
using AbstractionExceptions = LondonFhirService.Core.Abstractions.Models.Audits.Exceptions;
using ClientExceptions = LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnAddAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            Audit randomAudit = CreateRandomAudit();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.AddAuditAsync(It.IsAny<Audit>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask<Audit> addAuditTask =
                this.auditService.AddAuditAsync(randomAudit, TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), addAuditTask.AsTask) as Xeption;

            // then
            actualException.Should().BeEquivalentTo(expectedServiceException);

            VerifyLoggedOnceAsCategory(expectedServiceException);
        }

        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnRetrieveAllAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.RetrieveAllAuditsAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask<IQueryable<Audit>> retrieveAllTask =
                this.auditService.RetrieveAllAuditsAsync(TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), retrieveAllTask.AsTask) as Xeption;

            // then
            actualException.Should().BeEquivalentTo(expectedServiceException);

            VerifyLoggedOnceAsCategory(expectedServiceException);
        }

        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnBulkAddAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            List<Audit> randomAudits = CreateRandomAudits();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.BulkLogAuditsAsync(
                    It.IsAny<List<Audit>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(clientException);

            // when
            ValueTask bulkAddTask = this.auditService.BulkAddAuditsAsync(
                randomAudits,
                cancellationToken: TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), bulkAddTask.AsTask) as Xeption;

            // then
            actualException.Should().BeEquivalentTo(expectedServiceException);

            VerifyLoggedOnceAsCategory(expectedServiceException);
        }

        [Fact]
        public async Task ShouldNotTranslateCancellationOnAddAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            var operationCanceledException = new OperationCanceledException();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.AddAuditAsync(It.IsAny<Audit>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            // when
            Func<Task> addAudit = async () =>
                await this.auditService.AddAuditAsync(randomAudit, TestContext.Current.CancellationToken);

            // then
            // A caller that cancels gets the cancellation it asked for, not a service exception
            // it has to unwrap to find out what happened.
            (await addAudit.Should().ThrowAsync<OperationCanceledException>())
                .Which.Should().BeSameAs(operationCanceledException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Xeption>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldWrapAnUnexpectedExceptionAsAServiceExceptionOnAddAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            var serviceException = new Exception(GetRandomString());

            var failedAuditServiceException =
                new FailedAuditServiceException(
                    message: "Failed audit service error occurred, please contact support.",
                    innerException: serviceException);

            var expectedAuditServiceException =
                new AuditServiceException(
                    message: "Audit service error occurred, please contact support.",
                    innerException: failedAuditServiceException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.AddAuditAsync(It.IsAny<Audit>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Audit> addAuditTask =
                this.auditService.AddAuditAsync(randomAudit, TestContext.Current.CancellationToken);

            AuditServiceException actualAuditServiceException =
                await Assert.ThrowsAsync<AuditServiceException>(addAuditTask.AsTask);

            // then
            actualAuditServiceException.Should().BeEquivalentTo(expectedAuditServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedAuditServiceException))),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldLocaliseNotFoundIntoTheTypeTheControllerGuardsOnAsync()
        {
            // given
            Guid auditId = Guid.NewGuid();

            var clientNotFoundException =
                new ClientExceptions.AuditClientNotFoundException(
                    message: "Audit not found.",
                    innerException: new Xeption("Couldn't find audit."));

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.RetrieveAuditByIdAsync(auditId, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientNotFoundException);

            // when
            ValueTask<Audit> retrieveTask =
                this.auditService.RetrieveAuditByIdAsync(auditId, TestContext.Current.CancellationToken);

            AuditServiceValidationException actualException =
                await Assert.ThrowsAsync<AuditServiceValidationException>(retrieveTask.AsTask);

            // then
            // AuditsController answers 404 by testing the inner exception's type. A localisation
            // that loses the category turns every not-found into a 400.
            actualException.InnerException.Should().BeOfType<NotFoundAuditServiceException>();
        }

        [Fact]
        public async Task ShouldLocaliseAlreadyExistsIntoTheTypeTheControllerGuardsOnAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();

            var alreadyExistsException =
                new AbstractionExceptions.AlreadyExistsAuditException(
                    message: "Audit with the same id already exists.",
                    innerException: new Exception(),
                    data: null);

            var clientException =
                new ClientExceptions.AuditClientDependencyValidationException(
                    message: "Audit client dependency validation error occurred, fix errors and try again.",
                    innerException: alreadyExistsException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.AddAuditAsync(It.IsAny<Audit>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask<Audit> addTask =
                this.auditService.AddAuditAsync(randomAudit, TestContext.Current.CancellationToken);

            AuditServiceDependencyValidationException actualException =
                await Assert.ThrowsAsync<AuditServiceDependencyValidationException>(addTask.AsTask);

            // then
            // 409 Conflict rather than 400.
            actualException.InnerException.Should().BeOfType<AlreadyExistsAuditServiceException>();
        }

        [Fact]
        public async Task ShouldLocaliseLockedIntoTheTypeTheControllerGuardsOnAsync()
        {
            // given
            Guid auditId = Guid.NewGuid();

            var lockedException =
                new AbstractionExceptions.LockedAuditException(
                    message: "Locked audit record exception, please try again later.",
                    innerException: new Exception(),
                    data: null);

            var clientException =
                new ClientExceptions.AuditClientDependencyValidationException(
                    message: "Audit client dependency validation error occurred, fix errors and try again.",
                    innerException: lockedException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.RemoveAuditByIdAsync(auditId, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask<Audit> removeTask =
                this.auditService.RemoveAuditByIdAsync(auditId, TestContext.Current.CancellationToken);

            AuditServiceDependencyValidationException actualException =
                await Assert.ThrowsAsync<AuditServiceDependencyValidationException>(removeTask.AsTask);

            // then
            // 423 Locked rather than 400.
            actualException.InnerException.Should().BeOfType<LockedAuditServiceException>();
        }

        /// <summary>
        /// A dependency failure is logged as critical and everything else as an error, so the
        /// category the service assigned is checked rather than just that something was logged.
        /// </summary>
        private void VerifyLoggedOnceAsCategory(Xeption expectedServiceException)
        {
            if (expectedServiceException is AuditServiceDependencyException)
            {
                this.loggingBrokerMock.Verify(broker =>
                    broker.LogCriticalAsync(It.Is(SameExceptionAs(expectedServiceException))),
                        Times.Once);

                return;
            }

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedServiceException))),
                    Times.Once);
        }
    }
}
