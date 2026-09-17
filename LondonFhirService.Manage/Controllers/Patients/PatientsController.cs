// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RESTFulSense.Controllers;
using Xeptions;

using LondonFhirService.Core.Brokers.Correlations;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Manage.Models.Securities;
using LondonFhirService.Manage.Services.Foundations.Patients;

namespace LondonFhirService.Manage.Controllers.Patients
{
    /// <summary>
    /// The operator-facing side of $getstructuredrecord. It exists so someone holding a consumer's
    /// credentials can see exactly what that consumer would receive, without standing up a client
    /// of their own.
    ///
    /// It is a POST and not a GET although it reads: an NHS number and a date of birth are patient
    /// identifiable, and client credentials more so, so none of them belong in a query string that
    /// web server logs and browser history would keep.
    ///
    /// Authorised to the same audience as the rest of this host, which is reachable only from the
    /// business IP range. There is no [InvisibleApi] here - unlike the seed-and-tear-down verbs on
    /// the telemetry controllers, this endpoint is the feature.
    /// </summary>
    [Authorize(Roles = ManageRoles.AdministratorsAndUsers)]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : RESTFulController
    {
        private readonly IPatientService patientService;

        public PatientsController(IPatientService patientService) =>
            this.patientService = patientService;

        [HttpPost("getstructuredrecord")]
        public async ValueTask<ActionResult<string>> PostGetStructuredRecordAsync(
            [FromBody] StructuredRecordRequest structuredRecordRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                // An already-abandoned request does no provider work. This call can hold a
                // connection for the whole of the client's timeout, so short-circuiting a caller
                // who has gone is worth the line.
                cancellationToken.ThrowIfCancellationRequested();

                StructuredRecordResponse structuredRecordResponse =
                    await this.patientService.GetStructuredRecordAsync(
                        structuredRecordRequest,
                        cancellationToken);

                // On a header rather than in the body, because the body is the provider's payload
                // verbatim and wrapping it to carry one more field would change what the page is
                // showing. Same header name the Api answered with, so the id an operator sees here
                // is the id they would have seen calling that host directly.
                //
                // Omitted rather than sent empty when the Api did not supply one: a blank header
                // reads like an id the caller failed to parse.
                if (string.IsNullOrWhiteSpace(structuredRecordResponse.CorrelationId) is false)
                {
                    Response.Headers[CorrelationBroker.CorrelationIdHeaderName] =
                        structuredRecordResponse.CorrelationId;
                }

                // Content rather than Ok, and the media type named rather than negotiated.
                // Ok(string) is content-negotiated: a caller sending Accept: application/json
                // with no wildcard gets the payload serialised AS a JSON string - quoted and
                // escaped - which is not what this endpoint promises. The portal happens to send
                // a wildcard and the host happens to leave RespectBrowserAcceptHeader off, so it
                // reads as text today; both are someone else's setting to change.
                return Content(structuredRecordResponse.PayloadText, "text/plain");
            }
            catch (PatientServiceValidationException patientServiceValidationException)
            {
                return BadRequest(patientServiceValidationException.InnerException);
            }

            // Also a 400. The upstream refused something the caller supplied - credentials it did
            // not accept, a patient it does not hold - which is the operator's to correct, unlike
            // the dependency failure below.
            catch (PatientServiceDependencyValidationException patientServiceDependencyValidationException)
            {
                return UpstreamProblem(
                    statusCode: StatusCodes.Status400BadRequest,
                    exception: patientServiceDependencyValidationException,
                    responseBody: patientServiceDependencyValidationException.ResponseBody);
            }
            catch (PatientServiceDependencyException patientServiceDependencyException)
            {
                return UpstreamProblem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    exception: patientServiceDependencyException,
                    responseBody: patientServiceDependencyException.ResponseBody);
            }
            catch (PatientServiceException patientServiceException)
            {
                return InternalServerError(patientServiceException);
            }
        }

        /// <summary>
        /// Answers with what the upstream said, which on this screen is the answer - an operator
        /// testing a consumer's credentials needs the token endpoint's own invalid_client text,
        /// not a summary of it.
        ///
        /// The body is read from a property rather than from Exception.Data, and this is the only
        /// place it is read. It never passed through the logging broker on the way here, so the
        /// provider's OperationOutcome reaches the person who asked for it and nowhere else.
        /// </summary>
        private ObjectResult UpstreamProblem(int statusCode, Xeption exception, string responseBody)
        {
            var problemDetails = new ProblemDetails
            {
                Title = exception.Message,
                Status = statusCode,

                Detail = string.IsNullOrWhiteSpace(responseBody)
                    ? DescribeFailureWithoutUpstreamBody(exception)
                    : responseBody
            };

            return StatusCode(statusCode, problemDetails);
        }

        /// <summary>
        /// What to say when the upstream said nothing to relay.
        ///
        /// A timed out call has no response body by definition, so the operator was shown the
        /// wrapper's generic title over an empty detail - on the one screen whose purpose is
        /// explaining why a call failed. The timeout message is written by this service and worth
        /// relaying; the wrapper's own is not.
        ///
        /// Deliberately narrow. Relaying any inner message would surface whatever an arbitrary
        /// dependency failure happened to carry - including Xeption's default
        /// "Exception of type ... was thrown", which tells an operator nothing and reads like a
        /// leak of something internal.
        /// </summary>
        private static string DescribeFailureWithoutUpstreamBody(Xeption exception) =>
            exception.InnerException is TimedOutPatientServiceException timedOutException
                ? timedOutException.Message
                : null;
    }
}
