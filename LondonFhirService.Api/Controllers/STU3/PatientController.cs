// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Coordinations.Patients.Exceptions;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTFulSense.Controllers;

namespace LondonFhirService.Api.Controllers.STU3
{
    [Authorize]
    [ApiController]
    [Route("api/STU3/[controller]")]
    public class PatientController : RESTFulController
    {
        private readonly IStu3PatientCoordinationService patientCoordinationService;

        public PatientController(IStu3PatientCoordinationService patientCoordinationService)
        {
            this.patientCoordinationService = patientCoordinationService;
        }

        [HttpPost("$getstructuredrecord")]
        [Authorize(Roles = "Patients.GetStructuredRecord")]
        public async Task<ActionResult<Bundle>> GetStructuredRecord(
            [FromBody] Parameters parameters,
            CancellationToken cancellationToken)
        {
            try
            {
                string nhsNumber = ExtractStringParameter(parameters, "patientNHSNumber");
                string dateOfBirth = ExtractStringParameter(parameters, "patientDOB");

                bool? demographicsOnly =
                    ExtractBoolParameter(parameters, "demographicsOnly", partName: "includeDemographicsOnly");

                bool? includeInactivePatients =
                    ExtractBoolParameter(parameters, "includeInactivePatients", partName: "includeInactivePatients");

                string bundle = await this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients,
                    cancellationToken);

                return Content(bundle, "application/fhir+json");
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

        private static string ExtractStringParameter(Parameters parameters, string name)
        {
            var parameter = parameters?.Parameter?.FirstOrDefault(p => p.Name == name);

            if (parameter?.Value is FhirString fhirString)
            {
                return fhirString.Value;
            }

            if (parameter?.Value is Identifier identifier)
            {
                return identifier.Value;
            }

            return null;
        }

        private static bool? ExtractBoolParameter(Parameters parameters, string name, string partName = null)
        {
            var parameter = parameters?.Parameter?.FirstOrDefault(p => p.Name == name);

            if (partName == null)
            {
                return parameter?.Part?.FirstOrDefault()?.Value is FhirBoolean fhirBoolean ? fhirBoolean.Value : null;
            }

            return parameter?.Part?.FirstOrDefault(p => p.Name == partName)?.Value is FhirBoolean fhirBooleanPart
                ? fhirBooleanPart.Value
                : null;
        }
    }
}
