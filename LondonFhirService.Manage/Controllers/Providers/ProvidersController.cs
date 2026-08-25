// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Manage.Models.Securities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RESTFulSense.Controllers;

namespace LondonFhirService.Manage.Controllers.Providers
{
    /// <summary>
    /// The registry of upstream STU3 data sources. A provider row decides who the patient fan-out
    /// calls, which of them is primary, and the window each one is active for - so a bad write
    /// here changes what every consumer gets back, not just what one operator sees.
    ///
    /// That is why the roles split rather than sit at one level: reads are open to the same roles
    /// as the rest of this host, but create, update and delete are ManageAdmin only. The
    /// class-level attribute still applies to those verbs - ASP.NET Core requires every
    /// [Authorize] in scope to pass - so the method attribute narrows rather than replaces it.
    ///
    /// Unlike audits and metrics there is no [InvisibleApi] here. Providers are configuration an
    /// operator is expected to manage, not a compliance record or a telemetry span that only the
    /// acceptance suite should be seeding.
    /// </summary>
    [Authorize(Roles = ManageRoles.AdministratorsAndUsers)]
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController : RESTFulController
    {
        private readonly IProviderService providerService;

        public ProvidersController(IProviderService providerService) =>
            this.providerService = providerService;

        [HttpPost]
        [Authorize(Roles = ManageRoles.Administrators)]
        public async ValueTask<ActionResult<Provider>> PostProviderAsync([FromBody] Provider provider)
        {
            try
            {
                Provider addedProvider =
                    await this.providerService.AddProviderAsync(provider);

                return Created(addedProvider);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
            {
                return BadRequest(providerServiceValidationException.InnerException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
                when (providerServiceDependencyValidationException.InnerException
                    is AlreadyExistsProviderServiceException)
            {
                return Conflict(providerServiceDependencyValidationException.InnerException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
            {
                return BadRequest(providerServiceDependencyValidationException.InnerException);
            }
            catch (ProviderServiceDependencyException providerServiceDependencyException)
            {
                return InternalServerError(providerServiceDependencyException);
            }
            catch (ProviderServiceException providerServiceException)
            {
                return InternalServerError(providerServiceException);
            }
        }

        [HttpGet]
#if !DEBUG
        [EnableQuery(PageSize = 50)]
#endif
#if DEBUG
        [EnableQuery(PageSize = 5000)]
#endif
        public async ValueTask<ActionResult<IQueryable<Provider>>> Get()
        {
            try
            {
                IQueryable<Provider> retrievedProviders =
                    await this.providerService.RetrieveAllProvidersAsync();

                return Ok(retrievedProviders);
            }
            catch (ProviderServiceDependencyException providerServiceDependencyException)
            {
                return InternalServerError(providerServiceDependencyException);
            }
            catch (ProviderServiceException providerServiceException)
            {
                return InternalServerError(providerServiceException);
            }
        }

        [HttpGet("{providerId}")]
        public async ValueTask<ActionResult<Provider>> GetProviderByIdAsync(Guid providerId)
        {
            try
            {
                Provider provider = await this.providerService.RetrieveProviderByIdAsync(providerId);

                return Ok(provider);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
                when (providerServiceValidationException.InnerException is NotFoundProviderServiceException)
            {
                return NotFound(providerServiceValidationException.InnerException);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
            {
                return BadRequest(providerServiceValidationException.InnerException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
            {
                return BadRequest(providerServiceDependencyValidationException.InnerException);
            }
            catch (ProviderServiceDependencyException providerServiceDependencyException)
            {
                return InternalServerError(providerServiceDependencyException);
            }
            catch (ProviderServiceException providerServiceException)
            {
                return InternalServerError(providerServiceException);
            }
        }

        [HttpPut]
        [Authorize(Roles = ManageRoles.Administrators)]
        public async ValueTask<ActionResult<Provider>> PutProviderAsync([FromBody] Provider provider)
        {
            try
            {
                Provider modifiedProvider =
                    await this.providerService.ModifyProviderAsync(provider);

                return Ok(modifiedProvider);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
                when (providerServiceValidationException.InnerException is NotFoundProviderServiceException)
            {
                return NotFound(providerServiceValidationException.InnerException);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
            {
                return BadRequest(providerServiceValidationException.InnerException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
                when (providerServiceDependencyValidationException.InnerException
                    is AlreadyExistsProviderServiceException)
            {
                return Conflict(providerServiceDependencyValidationException.InnerException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
            {
                return BadRequest(providerServiceDependencyValidationException.InnerException);
            }
            catch (ProviderServiceDependencyException providerServiceDependencyException)
            {
                return InternalServerError(providerServiceDependencyException);
            }
            catch (ProviderServiceException providerServiceException)
            {
                return InternalServerError(providerServiceException);
            }
        }

        [HttpDelete("{providerId}")]
        [Authorize(Roles = ManageRoles.Administrators)]
        public async ValueTask<ActionResult<Provider>> DeleteProviderByIdAsync(Guid providerId)
        {
            try
            {
                Provider deletedProvider =
                    await this.providerService.RemoveProviderByIdAsync(providerId);

                return Ok(deletedProvider);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
                when (providerServiceValidationException.InnerException is NotFoundProviderServiceException)
            {
                return NotFound(providerServiceValidationException.InnerException);
            }
            catch (ProviderServiceValidationException providerServiceValidationException)
            {
                return BadRequest(providerServiceValidationException.InnerException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
                when (providerServiceDependencyValidationException.InnerException
                    is LockedProviderServiceException)
            {
                return Locked(providerServiceDependencyValidationException.InnerException);
            }
            catch (ProviderServiceDependencyValidationException providerServiceDependencyValidationException)
            {
                return BadRequest(providerServiceDependencyValidationException.InnerException);
            }
            catch (ProviderServiceDependencyException providerServiceDependencyException)
            {
                return InternalServerError(providerServiceDependencyException);
            }
            catch (ProviderServiceException providerServiceException)
            {
                return InternalServerError(providerServiceException);
            }
        }
    }
}
