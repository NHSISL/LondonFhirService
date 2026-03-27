// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using LondonFhirService.Core.Services.Foundations.FhirRecordDifferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RESTFulSense.Controllers;

namespace LondonFhirService.Manage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FhirRecordDifferencesController : RESTFulController
    {
        private readonly IFhirRecordDifferenceService fhirRecordDifferenceService;

        public FhirRecordDifferencesController(IFhirRecordDifferenceService fhirRecordDifferenceService) =>
            this.fhirRecordDifferenceService = fhirRecordDifferenceService;

        [HttpPost]
        [Authorize(Roles = "Administrators,FhirRecordDifferences.Create")]
        public async ValueTask<ActionResult<FhirRecordDifference>> PostFhirRecordDifferenceAsync(
            [FromBody] FhirRecordDifference fhirRecordDifference)
        {
            try
            {
                FhirRecordDifference addedFhirRecordDifference =
                    await this.fhirRecordDifferenceService.AddFhirRecordDifferenceAsync(fhirRecordDifference);

                return Created(addedFhirRecordDifference);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
            {
                return BadRequest(fhirRecordDifferenceValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyValidationException fhirRecordDifferenceDependencyValidationException)
                when (fhirRecordDifferenceDependencyValidationException.InnerException
                    is AlreadyExistsFhirRecordDifferenceException)
            {
                return Conflict(fhirRecordDifferenceDependencyValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyValidationException fhirRecordDifferenceDependencyValidationException)
            {
                return BadRequest(fhirRecordDifferenceDependencyValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyException fhirRecordDifferenceDependencyException)
            {
                return InternalServerError(fhirRecordDifferenceDependencyException);
            }
            catch (FhirRecordDifferenceServiceException fhirRecordDifferenceServiceException)
            {
                return InternalServerError(fhirRecordDifferenceServiceException);
            }
        }

        [HttpGet]
#if !DEBUG
        [EnableQuery(PageSize = 50)]
#endif
#if DEBUG
        [EnableQuery(PageSize = 5000)]
#endif
        [Authorize(Roles = "Administrators,FhirRecordDifferences.Read")]
        public async ValueTask<ActionResult<IQueryable<FhirRecordDifference>>> Get()
        {
            try
            {
                IQueryable<FhirRecordDifference> retrievedFhirRecordDifferences =
                    await this.fhirRecordDifferenceService.RetrieveAllFhirRecordDifferencesAsync();

                return Ok(retrievedFhirRecordDifferences);
            }
            catch (FhirRecordDifferenceDependencyException fhirRecordDifferenceDependencyException)
            {
                return InternalServerError(fhirRecordDifferenceDependencyException);
            }
            catch (FhirRecordDifferenceServiceException fhirRecordDifferenceServiceException)
            {
                return InternalServerError(fhirRecordDifferenceServiceException);
            }
        }

        [HttpGet("{fhirRecordDifferenceId}")]
        [Authorize(Roles = "Administrators,FhirRecordDifferences.Read")]
        public async ValueTask<ActionResult<FhirRecordDifference>> GetFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId)
        {
            try
            {
                FhirRecordDifference fhirRecordDifference = await this.fhirRecordDifferenceService
                    .RetrieveFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId);

                return Ok(fhirRecordDifference);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
                when (fhirRecordDifferenceValidationException.InnerException is NotFoundFhirRecordDifferenceException)
            {
                return NotFound(fhirRecordDifferenceValidationException.InnerException);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
            {
                return BadRequest(fhirRecordDifferenceValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyValidationException fhirRecordDifferenceDependencyValidationException)
            {
                return BadRequest(fhirRecordDifferenceDependencyValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyException fhirRecordDifferenceDependencyException)
            {
                return InternalServerError(fhirRecordDifferenceDependencyException);
            }
            catch (FhirRecordDifferenceServiceException fhirRecordDifferenceServiceException)
            {
                return InternalServerError(fhirRecordDifferenceServiceException);
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrators,FhirRecordDifferences.Update")]
        public async ValueTask<ActionResult<FhirRecordDifference>> PutFhirRecordDifferenceAsync(
            [FromBody] FhirRecordDifference fhirRecordDifference)
        {
            try
            {
                FhirRecordDifference modifiedFhirRecordDifference =
                    await this.fhirRecordDifferenceService.ModifyFhirRecordDifferenceAsync(fhirRecordDifference);

                return Ok(modifiedFhirRecordDifference);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
                when (fhirRecordDifferenceValidationException.InnerException is NotFoundFhirRecordDifferenceException)
            {
                return NotFound(fhirRecordDifferenceValidationException.InnerException);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
            {
                return BadRequest(fhirRecordDifferenceValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyValidationException fhirRecordDifferenceDependencyValidationException)
                when (fhirRecordDifferenceDependencyValidationException.InnerException
                    is AlreadyExistsFhirRecordDifferenceException)
            {
                return Conflict(fhirRecordDifferenceDependencyValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyValidationException fhirRecordDifferenceDependencyValidationException)
            {
                return BadRequest(fhirRecordDifferenceDependencyValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyException fhirRecordDifferenceDependencyException)
            {
                return InternalServerError(fhirRecordDifferenceDependencyException);
            }
            catch (FhirRecordDifferenceServiceException fhirRecordDifferenceServiceException)
            {
                return InternalServerError(fhirRecordDifferenceServiceException);
            }
        }

        [HttpDelete("{fhirRecordDifferenceId}")]
        [Authorize(Roles = "Administrators,FhirRecordDifferences.Delete")]
        public async ValueTask<ActionResult<FhirRecordDifference>> DeleteFhirRecordDifferenceByIdAsync(
            Guid fhirRecordDifferenceId)
        {
            try
            {
                FhirRecordDifference deletedFhirRecordDifference =
                    await this.fhirRecordDifferenceService.RemoveFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId);

                return Ok(deletedFhirRecordDifference);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
                when (fhirRecordDifferenceValidationException.InnerException is NotFoundFhirRecordDifferenceException)
            {
                return NotFound(fhirRecordDifferenceValidationException.InnerException);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
            {
                return BadRequest(fhirRecordDifferenceValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyValidationException fhirRecordDifferenceDependencyValidationException)
                when (fhirRecordDifferenceDependencyValidationException.InnerException
                    is LockedFhirRecordDifferenceException)
            {
                return Locked(fhirRecordDifferenceDependencyValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyValidationException fhirRecordDifferenceDependencyValidationException)
            {
                return BadRequest(fhirRecordDifferenceDependencyValidationException.InnerException);
            }
            catch (FhirRecordDifferenceDependencyException fhirRecordDifferenceDependencyException)
            {
                return InternalServerError(fhirRecordDifferenceDependencyException);
            }
            catch (FhirRecordDifferenceServiceException fhirRecordDifferenceServiceException)
            {
                return InternalServerError(fhirRecordDifferenceServiceException);
            }
        }
    }
}
