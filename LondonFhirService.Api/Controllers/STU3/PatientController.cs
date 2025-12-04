// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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

        [HttpPost("{id}/$everything")]
        [Authorize(Roles = "Patients.Everything")]
        public async Task<ActionResult<string>> Everything(
            string id,
            [FromBody] Parameters parameters,
            CancellationToken cancellationToken)
        {
            try
            {
                DateTimeOffset? start = ExtractDateTimeParameter(parameters, "start");
                DateTimeOffset? end = ExtractDateTimeParameter(parameters, "end");
                string typeFilter = ExtractStringParameter(parameters, "_type");
                DateTimeOffset? since = ExtractDateTimeParameter(parameters, "_since");
                int? count = ExtractIntParameter(parameters, "_count");

                string bundle = await this.patientCoordinationService.EverythingSerialisedAsync(
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

        [HttpPost("{nhsNumber}/$getstructuredrecord")]
        [Authorize(Roles = "Patients.GetStructuredRecord")]
        public async Task<ActionResult<Bundle>> GetStructuredRecord(
            string nhsNumber,
            [FromBody] Parameters parameters,
            CancellationToken cancellationToken)
        {
            try
            {
                DateTimeOffset? dateOfBirth = ExtractDateTimeParameter(parameters, "dateOfBirth");
                DateTime? dateOfBirthDateTime = dateOfBirth.HasValue ? dateOfBirth.Value.DateTime : null;
                bool? demographicsOnly = ExtractBoolParameter(parameters, "demographicsOnly");
                bool? includeInactivePatients = ExtractBoolParameter(parameters, "includeInactivePatients");

                string bundle = await this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                    nhsNumber,
                    dateOfBirthDateTime,
                    demographicsOnly,
                    includeInactivePatients,
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

        private static string ExtractStringParameter(Parameters parameters, string name)
        {
            var parameter = parameters?.Parameter?.FirstOrDefault(p => p.Name == name);

            return parameter?.Value is FhirString fhirString ? fhirString.Value : null;
        }

        private static DateTimeOffset? ExtractDateTimeParameter(Parameters parameters, string name)
        {
            var parameter = parameters?.Parameter?.FirstOrDefault(p => p.Name == name);

            if (parameter?.Value is FhirDateTime fhirDateTime && fhirDateTime.Value != null)
            {
                return DateTimeOffset.Parse(fhirDateTime.Value);
            }

            if (parameter?.Value is Date date && date.Value != null)
            {
                return DateTimeOffset.Parse(date.Value);
            }

            return null;
        }

        private static int? ExtractIntParameter(Parameters parameters, string name)
        {
            var parameter = parameters?.Parameter?.FirstOrDefault(p => p.Name == name);

            return parameter?.Value is Integer integer ? integer.Value : null;
        }

        private static bool? ExtractBoolParameter(Parameters parameters, string name)
        {
            var parameter = parameters?.Parameter?.FirstOrDefault(p => p.Name == name);

            return parameter?.Value is FhirBoolean fhirBoolean ? fhirBoolean.Value : null;
        }
    }
}
