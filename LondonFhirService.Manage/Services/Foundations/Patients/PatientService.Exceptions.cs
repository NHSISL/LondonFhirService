// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Manage.Services.Foundations.Patients
{
    internal partial class PatientService
    {
        private delegate ValueTask<string> ReturningStringFunction();

        private static async ValueTask<string> TryCatch(
            ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (NullPatientServiceException nullPatientServiceException)
            {
                throw CreateValidationException(nullPatientServiceException);
            }
            catch (InvalidPatientServiceException invalidPatientServiceException)
            {
                throw CreateValidationException(invalidPatientServiceException);
            }

            // Ordered before the plain cancellation catch on purpose. A timeout surfaces as an
            // OperationCanceledException wrapping a TimeoutException, and catching cancellation
            // first would report every timed out provider call as an abandoned request.
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.InnerException is TimeoutException)
            {
                var timedOutPatientServiceException =
                    new TimedOutPatientServiceException(
                        message: "Patient request timed out, please try again.",
                        innerException: operationCanceledException,
                        data: operationCanceledException.Data);

                throw CreateDependencyException(timedOutPatientServiceException);
            }
            catch (TimeoutException timeoutException)
            {
                var timedOutPatientServiceException =
                    new TimedOutPatientServiceException(
                        message: "Patient request timed out, please try again.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw CreateDependencyException(timedOutPatientServiceException);
            }
            // Rethrown, never wrapped. Cancellation is not a failure - it is the caller saying it
            // no longer wants the answer - so it has to reach them as itself rather than as a
            // dependency error that reads like the provider broke. This catch exists at all only
            // to stop the catch-all below swallowing it.
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidAccessTokenPatientServiceException invalidAccessTokenPatientServiceException)
            {
                throw CreateDependencyException(invalidAccessTokenPatientServiceException);
            }

            // A malformed token payload is the authorisation server answering with something this
            // service cannot read, which is a dependency failure rather than an unexpected fault
            // in this process.
            catch (JsonException jsonException)
            {
                var failedPatientDependencyException =
                    new FailedPatientDependencyException(
                        message: "Failed patient dependency error occurred, contact support.",
                        innerException: jsonException,
                        data: jsonException.Data);

                throw CreateDependencyException(failedPatientDependencyException);
            }
            catch (HttpRequestException httpRequestException)
            {
                var failedPatientDependencyException =
                    new FailedPatientDependencyException(
                        message: "Failed patient dependency error occurred, contact support.",
                        innerException: httpRequestException,
                        data: httpRequestException.Data);

                throw CreateDependencyException(failedPatientDependencyException);
            }
            catch (Exception exception)
            {
                var failedPatientServiceException =
                    new FailedPatientServiceException(
                        message: "Failed patient service error occurred, contact support.",
                        innerException: exception);

                throw CreateServiceException(failedPatientServiceException);
            }
        }

        private static PatientServiceValidationException CreateValidationException(
            Xeption exception)
        {
            return new PatientServiceValidationException(
                message: "Patient validation error occurred, please fix errors and try again.",
                innerException: exception);
        }

        private static PatientServiceDependencyException CreateDependencyException(
            Xeption exception)
        {
            return new PatientServiceDependencyException(
                message: "Patient dependency error occurred, contact support.",
                innerException: exception);
        }

        private static PatientServiceException CreateServiceException(
            Xeption exception)
        {
            return new PatientServiceException(
                message: "Patient service error occurred, contact support.",
                innerException: exception);
        }
    }
}
