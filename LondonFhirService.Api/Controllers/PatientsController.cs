// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Services.Coordinations.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LondonFhirService.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/R4/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientCoordinationService patientCoordinationService;

        public PatientsController(IPatientCoordinationService patientCoordinationService)
        {
            this.patientCoordinationService = patientCoordinationService;
        }

        [HttpGet]
        public async Task<ActionResult<Bundle>> GetRecord(
            string id,
            [FromQuery] DateTimeOffset? start = null,
            [FromQuery] DateTimeOffset? end = null,
            [FromQuery] string _type = null,
            [FromQuery] DateTimeOffset? _since = null,
            [FromQuery] int? _count = null,
            CancellationToken cancellationToken = default)
        {
            Bundle bundle = await this.patientCoordinationService.Everything(
                id,
                start,
                end,
                typeFilter: _type,
                since: _since,
                count: _count,
                cancellationToken);

            return Ok(bundle);
        }
    }
}
