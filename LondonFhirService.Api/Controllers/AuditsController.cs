// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Attrify.Attributes;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.Audits.Exceptions;
using LondonFhirService.Core.Services.Foundations.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RESTFulSense.Controllers;

namespace LondonFhirService.Api.Controllers
{
    [Authorize(Roles = "Administrators,Users")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditsController : RESTFulController
    {
        private readonly IAuditService auditService;

        public AuditsController(IAuditService auditService) =>
            this.auditService = auditService;

        [InvisibleApi]
        [HttpPost]
        public async ValueTask<ActionResult<Audit>> PostAuditAsync([FromBody] Audit audit)
        {
            try
            {
                Audit addedAudit =
                    await this.auditService.AddAuditAsync(audit);

                return Created(addedAudit);
            }
            catch (AuditServiceValidationException auditServiceValidationException)
            {
                return BadRequest(auditServiceValidationException.InnerException);
            }
            catch (AuditServiceDependencyValidationException auditServiceDependencyValidationException)
               when (auditServiceDependencyValidationException.InnerException is AlreadyExistsAuditServiceException)
            {
                return Conflict(auditServiceDependencyValidationException.InnerException);
            }
            catch (AuditServiceDependencyValidationException auditServiceDependencyValidationException)
            {
                return BadRequest(auditServiceDependencyValidationException.InnerException);
            }
            catch (AuditServiceDependencyException auditServiceDependencyException)
            {
                return InternalServerError(auditServiceDependencyException);
            }
            catch (AuditServiceException auditServiceException)
            {
                return InternalServerError(auditServiceException);
            }
        }

        [HttpGet]
#if !DEBUG
        [EnableQuery(PageSize = 50)]
#endif
#if DEBUG
        [EnableQuery(PageSize = 5000)]
#endif
        public async ValueTask<ActionResult<IQueryable<Audit>>> Get()
        {
            try
            {
                IQueryable<Audit> retrievedAudits =
                    await this.auditService.RetrieveAllAuditsAsync();

                return Ok(retrievedAudits);
            }
            catch (AuditServiceDependencyException auditServiceDependencyException)
            {
                return InternalServerError(auditServiceDependencyException);
            }
            catch (AuditServiceException auditServiceException)
            {
                return InternalServerError(auditServiceException);
            }
        }

        [HttpGet("{auditId}")]
        public async ValueTask<ActionResult<Audit>> GetAuditByIdAsync(Guid auditId)
        {
            try
            {
                Audit audit = await this.auditService.RetrieveAuditByIdAsync(auditId);

                return Ok(audit);
            }
            catch (AuditServiceValidationException auditServiceValidationException)
                when (auditServiceValidationException.InnerException is NotFoundAuditServiceException)
            {
                return NotFound(auditServiceValidationException.InnerException);
            }
            catch (AuditServiceValidationException auditServiceValidationException)
            {
                return BadRequest(auditServiceValidationException.InnerException);
            }
            catch (AuditServiceDependencyValidationException auditServiceDependencyValidationException)
            {
                return BadRequest(auditServiceDependencyValidationException.InnerException);
            }
            catch (AuditServiceDependencyException auditServiceDependencyException)
            {
                return InternalServerError(auditServiceDependencyException);
            }
            catch (AuditServiceException auditServiceException)
            {
                return InternalServerError(auditServiceException);
            }
        }

        [InvisibleApi]
        [HttpPut]
        public async ValueTask<ActionResult<Audit>> PutAuditAsync([FromBody] Audit audit)
        {
            try
            {
                Audit modifiedAudit =
                    await this.auditService.ModifyAuditAsync(audit);

                return Ok(modifiedAudit);
            }
            catch (AuditServiceValidationException auditServiceValidationException)
                when (auditServiceValidationException.InnerException is NotFoundAuditServiceException)
            {
                return NotFound(auditServiceValidationException.InnerException);
            }
            catch (AuditServiceValidationException auditServiceValidationException)
            {
                return BadRequest(auditServiceValidationException.InnerException);
            }
            catch (AuditServiceDependencyValidationException auditServiceDependencyValidationException)
               when (auditServiceDependencyValidationException.InnerException is AlreadyExistsAuditServiceException)
            {
                return Conflict(auditServiceDependencyValidationException.InnerException);
            }
            catch (AuditServiceDependencyValidationException auditServiceDependencyValidationException)
            {
                return BadRequest(auditServiceDependencyValidationException.InnerException);
            }
            catch (AuditServiceDependencyException auditServiceDependencyException)
            {
                return InternalServerError(auditServiceDependencyException);
            }
            catch (AuditServiceException auditServiceException)
            {
                return InternalServerError(auditServiceException);
            }
        }

        [InvisibleApi]
        [HttpDelete("{auditId}")]
        public async ValueTask<ActionResult<Audit>> DeleteAuditByIdAsync(Guid auditId)
        {
            try
            {
                Audit deletedAudit =
                    await this.auditService.RemoveAuditByIdAsync(auditId);

                return Ok(deletedAudit);
            }
            catch (AuditServiceValidationException auditServiceValidationException)
                when (auditServiceValidationException.InnerException is NotFoundAuditServiceException)
            {
                return NotFound(auditServiceValidationException.InnerException);
            }
            catch (AuditServiceValidationException auditServiceValidationException)
            {
                return BadRequest(auditServiceValidationException.InnerException);
            }
            catch (AuditServiceDependencyValidationException auditServiceDependencyValidationException)
                when (auditServiceDependencyValidationException.InnerException is LockedAuditServiceException)
            {
                return Locked(auditServiceDependencyValidationException.InnerException);
            }
            catch (AuditServiceDependencyValidationException auditServiceDependencyValidationException)
            {
                return BadRequest(auditServiceDependencyValidationException.InnerException);
            }
            catch (AuditServiceDependencyException auditServiceDependencyException)
            {
                return InternalServerError(auditServiceDependencyException);
            }
            catch (AuditServiceException auditServiceException)
            {
                return InternalServerError(auditServiceException);
            }
        }
    }
}
