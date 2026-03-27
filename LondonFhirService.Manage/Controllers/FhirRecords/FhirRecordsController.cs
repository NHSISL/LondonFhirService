// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using LondonFhirService.Core.Services.Foundations.FhirRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RESTFulSense.Controllers;

namespace LondonFhirService.Manage.Controllers.FhirRecords
{
    [ApiController]
    [Route("api/[controller]")]
    public class FhirRecordsController : RESTFulController
    {
        private readonly IFhirRecordService fhirRecordService;

        public FhirRecordsController(IFhirRecordService fhirRecordService) =>
            this.fhirRecordService = fhirRecordService;

        [HttpPost]
        //[InvisibleApi]
        [Authorize(Roles = "Administrators,FhirRecords.Create")]
        public async ValueTask<ActionResult<FhirRecord>> PostFhirRecordAsync([FromBody] FhirRecord fhirRecord)
        {
            try
            {
                FhirRecord addedFhirRecord =
                    await this.fhirRecordService.AddFhirRecordAsync(fhirRecord);

                return Created(addedFhirRecord);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
            {
                return BadRequest(fhirRecordValidationException.InnerException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
                when (fhirRecordDependencyValidationException.InnerException
                    is AlreadyExistsFhirRecordException)
            {
                return Conflict(fhirRecordDependencyValidationException.InnerException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
            {
                return BadRequest(fhirRecordDependencyValidationException.InnerException);
            }
            catch (FhirRecordDependencyException fhirRecordDependencyException)
            {
                return InternalServerError(fhirRecordDependencyException);
            }
            catch (FhirRecordServiceException fhirRecordServiceException)
            {
                return InternalServerError(fhirRecordServiceException);
            }
        }

        [HttpGet]
#if !DEBUG
        [EnableQuery(PageSize = 50)]
#endif
#if DEBUG
        [EnableQuery(PageSize = 5000)]
#endif
        //[InvisibleApi]
        [Authorize(Roles = "Administrators,FhirRecords.Read")]
        public async ValueTask<ActionResult<IQueryable<FhirRecord>>> Get()
        {
            try
            {
                IQueryable<FhirRecord> retrievedFhirRecords =
                    await this.fhirRecordService.RetrieveAllFhirRecordsAsync();

                return Ok(retrievedFhirRecords);
            }
            catch (FhirRecordDependencyException fhirRecordDependencyException)
            {
                return InternalServerError(fhirRecordDependencyException);
            }
            catch (FhirRecordServiceException fhirRecordServiceException)
            {
                return InternalServerError(fhirRecordServiceException);
            }
        }

        [HttpGet("{fhirRecordId}")]
        //[InvisibleApi]
        [Authorize(Roles = "Administrators,FhirRecords.Read")]
        public async ValueTask<ActionResult<FhirRecord>> GetFhirRecordByIdAsync(Guid fhirRecordId)
        {
            try
            {
                FhirRecord fhirRecord = await this.fhirRecordService.RetrieveFhirRecordByIdAsync(fhirRecordId);

                return Ok(fhirRecord);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
                when (fhirRecordValidationException.InnerException is NotFoundFhirRecordException)
            {
                return NotFound(fhirRecordValidationException.InnerException);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
            {
                return BadRequest(fhirRecordValidationException.InnerException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
            {
                return BadRequest(fhirRecordDependencyValidationException.InnerException);
            }
            catch (FhirRecordDependencyException fhirRecordDependencyException)
            {
                return InternalServerError(fhirRecordDependencyException);
            }
            catch (FhirRecordServiceException fhirRecordServiceException)
            {
                return InternalServerError(fhirRecordServiceException);
            }
        }

        [HttpPut]
        //[InvisibleApi]
        [Authorize(Roles = "Administrators,FhirRecords.Update")]
        public async ValueTask<ActionResult<FhirRecord>> PutFhirRecordAsync([FromBody] FhirRecord fhirRecord)
        {
            try
            {
                FhirRecord modifiedFhirRecord =
                    await this.fhirRecordService.ModifyFhirRecordAsync(fhirRecord);

                return Ok(modifiedFhirRecord);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
                when (fhirRecordValidationException.InnerException is NotFoundFhirRecordException)
            {
                return NotFound(fhirRecordValidationException.InnerException);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
            {
                return BadRequest(fhirRecordValidationException.InnerException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
                when (fhirRecordDependencyValidationException.InnerException is AlreadyExistsFhirRecordException)
            {
                return Conflict(fhirRecordDependencyValidationException.InnerException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
            {
                return BadRequest(fhirRecordDependencyValidationException.InnerException);
            }
            catch (FhirRecordDependencyException fhirRecordDependencyException)
            {
                return InternalServerError(fhirRecordDependencyException);
            }
            catch (FhirRecordServiceException fhirRecordServiceException)
            {
                return InternalServerError(fhirRecordServiceException);
            }
        }

        [HttpDelete("{fhirRecordId}")]
        //[InvisibleApi]
        [Authorize(Roles = "Administrators,FhirRecords.Delete")]
        public async ValueTask<ActionResult<FhirRecord>> DeleteFhirRecordByIdAsync(Guid fhirRecordId)
        {
            try
            {
                FhirRecord deletedFhirRecord =
                    await this.fhirRecordService.RemoveFhirRecordByIdAsync(fhirRecordId);

                return Ok(deletedFhirRecord);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
                when (fhirRecordValidationException.InnerException is NotFoundFhirRecordException)
            {
                return NotFound(fhirRecordValidationException.InnerException);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
            {
                return BadRequest(fhirRecordValidationException.InnerException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
                when (fhirRecordDependencyValidationException.InnerException is LockedFhirRecordException)
            {
                return Locked(fhirRecordDependencyValidationException.InnerException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
            {
                return BadRequest(fhirRecordDependencyValidationException.InnerException);
            }
            catch (FhirRecordDependencyException fhirRecordDependencyException)
            {
                return InternalServerError(fhirRecordDependencyException);
            }
            catch (FhirRecordServiceException fhirRecordServiceException)
            {
                return InternalServerError(fhirRecordServiceException);
            }
        }
    }
}
