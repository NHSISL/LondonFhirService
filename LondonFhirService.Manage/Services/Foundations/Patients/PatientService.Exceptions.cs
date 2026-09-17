// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Manage.Services.Foundations.Patients
{
    internal partial class PatientService
    {
        private delegate ValueTask<string> ReturningStringFunction();

        private async ValueTask<string> TryCatch(
            ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (NullPatientServiceException nullPatientServiceException)
            {
                throw await CreateAndLogValidationException(nullPatientServiceException);
            }
            catch (InvalidPatientServiceException invalidPatientServiceException)
            {
                throw await CreateAndLogValidationException(invalidPatientServiceException);
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

                throw await CreateAndLogDependencyException(timedOutPatientServiceException);
            }
            catch (TimeoutException timeoutException)
            {
                var timedOutPatientServiceException =
                    new TimedOutPatientServiceException(
                        message: "Patient request timed out, please try again.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyException(timedOutPatientServiceException);
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
                throw await CreateAndLogDependencyException(invalidAccessTokenPatientServiceException);
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

                throw await CreateAndLogDependencyException(failedPatientDependencyException);
            }
            // A 4xx is the upstream judging the request rather than failing at it, and on this
            // screen that is nearly always the operator's own credentials being rejected. Reporting
            // it as a dependency failure turned "the token endpoint said invalid_client" into a 500
            // and an invitation to contact support, about the one thing the page exists to test.
            catch (HttpRequestException httpRequestException)
                when (IsUpstreamRefusal(httpRequestException.StatusCode))
            {
                var failedPatientDependencyValidationException =
                    new FailedPatientDependencyValidationException(
                        message: "Failed patient dependency validation error occurred, " +
                            "please fix the errors and try again.",
                        innerException: httpRequestException,
                        data: httpRequestException.Data);

                throw await CreateAndLogDependencyValidationException(failedPatientDependencyValidationException);
            }
            catch (HttpRequestException httpRequestException)
            {
                var failedPatientDependencyException =
                    new FailedPatientDependencyException(
                        message: "Failed patient dependency error occurred, contact support.",
                        innerException: httpRequestException,
                        data: httpRequestException.Data);

                throw await CreateAndLogDependencyException(failedPatientDependencyException);
            }
            catch (Exception exception)
            {
                var failedPatientServiceException =
                    new FailedPatientServiceException(
                        message: "Failed patient service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedPatientServiceException);
            }
        }

        /// <summary>
        /// A status the upstream chose, rather than one the transport produced. Null means the
        /// request never got an answer - a DNS failure, a refused connection - which is not the
        /// caller's to fix and stays a dependency failure.
        ///
        /// Every 4xx is treated the same way. Translating them individually would mean deciding
        /// that the provider's 404 is this endpoint's 404, and it is not: the patient was not
        /// found, the endpoint was.
        /// </summary>
        private static bool IsUpstreamRefusal(HttpStatusCode? statusCode) =>
            statusCode is not null
                && (int)statusCode >= 400
                && (int)statusCode <= 499;

        private async ValueTask<PatientServiceValidationException> CreateAndLogValidationException(
            Xeption exception)
        {
            var patientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceValidationException);

            return patientServiceValidationException;
        }

        private async ValueTask<PatientServiceDependencyValidationException>
            CreateAndLogDependencyValidationException(Xeption exception)
        {
            var patientServiceDependencyValidationException =
                new PatientServiceDependencyValidationException(
                    message: "Patient dependency validation error occurred, " +
                        "please fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceDependencyValidationException);

            return patientServiceDependencyValidationException;
        }

        private async ValueTask<PatientServiceDependencyException> CreateAndLogDependencyException(
            Xeption exception)
        {
            var patientServiceDependencyException =
                new PatientServiceDependencyException(
                    message: "Patient dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceDependencyException);

            return patientServiceDependencyException;
        }

        private async ValueTask<PatientServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var patientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(patientServiceException);

            return patientServiceException;
        }

    }
}
