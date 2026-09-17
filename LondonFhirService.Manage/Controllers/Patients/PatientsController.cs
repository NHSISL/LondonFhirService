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
                string structuredRecord = await this.patientService.GetStructuredRecordAsync(
                    structuredRecordRequest,
                    cancellationToken);

                return Ok(structuredRecord);
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
                Detail = responseBody
            };

            return StatusCode(statusCode, problemDetails);
        }
    }
}
