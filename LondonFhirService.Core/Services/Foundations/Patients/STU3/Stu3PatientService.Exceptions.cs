// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Patients.STU3
{
    internal partial class Stu3PatientService
    {
        private delegate ValueTask<List<Bundle>> ReturningBundleListFunction();
        private delegate ValueTask<List<(string, string)>> ReturningStringListFunction();

        private async ValueTask<List<Bundle>> TryCatch(
            ReturningBundleListFunction returningBundleListFunction)
        {
            try
            {
                return await returningBundleListFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidArgumentsPatientServiceException invalidArgumentsPatientServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentsPatientServiceException);
            }
            catch (Exception exception)
               when ((exception is IFhirValidationException
                   || exception is IFhirDependencyException
                   || exception is IFhirServiceException)
                  && exception.InnerException?.InnerException is OperationCanceledException cancelledInnerException
                  && cancelledInnerException.CancellationToken.IsCancellationRequested)
            {
                // The provider library has already put the caller's cancellation inside one of
                // its own categories. It is unwrapped rather than wrapped a second time, so the
                // caller gets back the cancellation it caused, and nothing is logged as an error.
                throw cancelledInnerException;
            }
            // A cancellation whose token was never cancelled is the transport giving up - an
            // HttpClient timeout or a dropped connection - rather than the caller, so it stays a
            // failure.
            catch (Exception exception)
               when ((exception is IFhirValidationException
                   || exception is IFhirDependencyException
                   || exception is IFhirServiceException)
                  && exception.InnerException?.InnerException is OperationCanceledException)
            {
                var networkOperationCanceledException =
                    (OperationCanceledException)exception.InnerException.InnerException;

                var networkPatientServiceException =
                    new NetworkPatientServiceException(
                        message: "Network connectivity failure occurred, please check connection and try again.",
                        innerException: networkOperationCanceledException,
                        data: networkOperationCanceledException.Data);

                throw await CreateAndLogServiceExceptionAsync(networkPatientServiceException);
            }
            catch (Exception exception)
               when (exception is IFhirValidationException)
            {
                var failedPatientDependencyValidationException =
                    new FailedPatientDependencyValidationException(
                        message: "Failed patient dependency validation error occurred, please try again.",
                        innerException: exception.InnerException,
                        data: exception.Data);

                throw await CreateAndLogDependencyValidationException(
                    failedPatientDependencyValidationException);
            }
            catch (Exception exception)
               when (exception is IFhirDependencyException
                   || exception is IFhirServiceException)
            {
                var failedPatientDependencyException =
                    new FailedPatientDependencyException(
                        message: "Failed patient dependency error occurred, contact support.",
                        innerException: exception.InnerException,
                        data: exception.Data);

                throw await CreateAndLogDependencyException(failedPatientDependencyException);
            }
            catch (Exception exception)
            {
                var failedPatientServiceException =
                    new FailedPatientServiceException(
                        message: "Failed patient service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientServiceException);
            }
        }

        private async ValueTask<List<(string, string)>> TryCatch(
            ReturningStringListFunction returningStringListFunction)
        {
            try
            {
                return await returningStringListFunction();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidArgumentsPatientServiceException invalidArgumentsPatientServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentsPatientServiceException);
            }
            catch (Exception exception)
               when ((exception is IFhirValidationException
                   || exception is IFhirDependencyException
                   || exception is IFhirServiceException)
                  && exception.InnerException?.InnerException is OperationCanceledException cancelledInnerException
                  && cancelledInnerException.CancellationToken.IsCancellationRequested)
            {
                // The provider library has already put the caller's cancellation inside one of
                // its own categories. It is unwrapped rather than wrapped a second time, so the
                // caller gets back the cancellation it caused, and nothing is logged as an error.
                throw cancelledInnerException;
            }
            // A cancellation whose token was never cancelled is the transport giving up - an
            // HttpClient timeout or a dropped connection - rather than the caller, so it stays a
            // failure.
            catch (Exception exception)
               when ((exception is IFhirValidationException
                   || exception is IFhirDependencyException
                   || exception is IFhirServiceException)
                  && exception.InnerException?.InnerException is OperationCanceledException)
            {
                var networkOperationCanceledException =
                    (OperationCanceledException)exception.InnerException.InnerException;

                var networkPatientServiceException =
                    new NetworkPatientServiceException(
                        message: "Network connectivity failure occurred, please check connection and try again.",
                        innerException: networkOperationCanceledException,
                        data: networkOperationCanceledException.Data);

                throw await CreateAndLogServiceExceptionAsync(networkPatientServiceException);
            }
            catch (Exception exception)
               when (exception is IFhirValidationException)
            {
                var failedPatientDependencyValidationException =
                    new FailedPatientDependencyValidationException(
                        message: "Failed patient dependency validation error occurred, please try again.",
                        innerException: exception.InnerException,
                        data: exception.Data);

                throw await CreateAndLogDependencyValidationException(
                    failedPatientDependencyValidationException);
            }
            catch (Exception exception)
               when (exception is IFhirDependencyException
                   || exception is IFhirServiceException)
            {
                var failedPatientDependencyException =
                    new FailedPatientDependencyException(
                        message: "Failed patient dependency error occurred, contact support.",
                        innerException: exception.InnerException,
                        data: exception.Data);

                throw await CreateAndLogDependencyException(failedPatientDependencyException);
            }
            catch (Exception exception)
            {
                var failedPatientServiceException =
                    new FailedPatientServiceException(
                        message: "Failed patient service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientServiceException);
            }
        }

        private async ValueTask<PatientServiceValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var patientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, " +
                        "please fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceValidationException);

            return patientServiceValidationException;
        }

        private async ValueTask<PatientServiceDependencyValidationException> CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var patientServiceDependencyValidationException =
                new PatientServiceDependencyValidationException(
                    message: "Patient service dependency validation error occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceDependencyValidationException);

            return patientServiceDependencyValidationException;
        }

        private async ValueTask<PatientServiceDependencyException> CreateAndLogDependencyException(Xeption exception)
        {
            var patientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceDependencyException);

            return patientServiceDependencyException;
        }

        private async ValueTask<PatientServiceException> CreateAndLogServiceExceptionAsync(
           Xeption exception)
        {
            var patientServiceException = new PatientServiceException(
                message: "Patient service error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceException);

            return patientServiceException;
        }
    }
}
