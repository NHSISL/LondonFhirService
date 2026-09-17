// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Manage.Models.Securities;
using LondonFhirService.Manage.Services.Foundations.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTFulSense.Controllers;

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
            catch (PatientServiceDependencyException patientServiceDependencyException)
            {
                return InternalServerError(patientServiceDependencyException);
            }
            catch (PatientServiceException patientServiceException)
            {
                return InternalServerError(patientServiceException);
            }
        }
    }
}
