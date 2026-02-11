// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Audits
{
    public partial class AuditServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveByIdIfSqlErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedAuditStorageException =
                new FailedStorageAuditServiceException(
                    message: "Failed audit storage error occurred, please contact support.",
                    innerException: sqlException);

            var expectedAuditDependencyException =
                new AuditServiceDependencyException(
                    message: "Audit dependency error occurred, please contact support.",
                    innerException: failedAuditStorageException);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Audit> retrieveAuditByIdTask =
                this.auditService.RetrieveAuditByIdAsync(someId);

            AuditServiceDependencyException actualAuditDependencyException =
                await Assert.ThrowsAsync<AuditServiceDependencyException>(
                    retrieveAuditByIdTask.AsTask);

            // then
            actualAuditDependencyException.Should()
                .BeEquivalentTo(expectedAuditDependencyException);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedAuditDependencyException))),
                        Times.Once);

            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedAuditServiceException =
                new FailedAuditServiceException(
                    message: "Failed audit service error occurred, please contact support.",
                    innerException: serviceException);

            var expectedAuditServiceException =
                new AuditServiceException(
                    message: "Audit service error occurred, please contact support.",
                    innerException: failedAuditServiceException);

            this.storageBrokerFactoryMock.Setup(broker =>
                broker.CreateDbContextAsync())
                    .ReturnsAsync(this.storageBrokerMock.Object as StorageBroker);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Audit> retrieveAuditByIdTask =
                this.auditService.RetrieveAuditByIdAsync(someId);

            AuditServiceException actualAuditServiceException =
                await Assert.ThrowsAsync<AuditServiceException>(
                    retrieveAuditByIdTask.AsTask);

            // then
            actualAuditServiceException.Should()
                .BeEquivalentTo(expectedAuditServiceException);

            this.storageBrokerFactoryMock.Verify(broker =>
                broker.CreateDbContextAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAuditByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAuditServiceException))),
                        Times.Once);

            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}