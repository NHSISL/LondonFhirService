// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using LondonFhirService.Core.Services.Coordinations.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTFulSense.Controllers;

namespace LondonFhirService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/R4/[controller]")]
    public class PatientsController : RESTFulController
    {
        private readonly IPatientCoordinationService patientCoordinationService;

        public PatientsController(IPatientCoordinationService patientCoordinationService)
        {
            this.patientCoordinationService = patientCoordinationService;
        }

        [HttpGet]
        [Authorize(Policy = "Patient.Everything")]
        public async Task<ActionResult<Bundle>> Everything(
            string id,
            [FromQuery] DateTimeOffset? start = null,
            [FromQuery] DateTimeOffset? end = null,
            [FromQuery] string typeFilter = null,
            [FromQuery] DateTimeOffset? since = null,
            [FromQuery] int? count = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Bundle bundle = await this.patientCoordinationService.Everything(
                    id,
                    start,
                    end,
                    typeFilter,
                    since,
                    count,
                    cancellationToken);

                return Ok(bundle);
            }
            catch (PatientCoordinationValidationException patientCoordinationValidationException)
            {
                return BadRequest(patientCoordinationValidationException.InnerException);
            }
            catch (PatientCoordinationDependencyValidationException
                   patientCoordinationDependencyValidationException)
            {
                return BadRequest(patientCoordinationDependencyValidationException.InnerException);
            }
            catch (PatientCoordinationDependencyException patientCoordinationDependencyException)
            {
                return InternalServerError(patientCoordinationDependencyException);
            }
            catch (PatientCoordinationServiceException patientCoordinationServiceException)
            {
                return InternalServerError(patientCoordinationServiceException);
            }
        }
    }
}
