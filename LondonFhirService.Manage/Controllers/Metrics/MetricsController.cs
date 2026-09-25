// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Attrify.Attributes;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Metrics.Exceptions;
using LondonFhirService.Core.Services.Foundations.Metrics;
using LondonFhirService.Core.Services.Orchestrations.Metrics;
using LondonFhirService.Manage.Models.Securities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RESTFulSense.Controllers;

namespace LondonFhirService.Manage.Controllers.Metrics
{
    /// <summary>
    /// The metric counterpart to AuditsController. Metric rows name the consumer and the target
    /// of every call, so this lives on the internal management host rather than the public API -
    /// Manage is reachable only from the business IP range.
    ///
    /// Reads are open to the same roles as the rest of this host. Create and delete carry
    /// [InvisibleApi], which the middleware registered on this host enforces: they are unroutable
    /// without the key header, so they exist to let the acceptance suite seed and tear down a
    /// database rather than as an operator-facing way to rewrite telemetry.
    ///
    /// There is no PUT. A metric is a span of work that already happened, so the table is
    /// append-only and no update path exists beneath this controller to expose.
    /// </summary>
    [Authorize(Roles = ManageRoles.AdministratorsAndUsers)]
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : RESTFulController
    {
        private readonly IMetricService metricService;
        private readonly IMetricOrchestrationService metricOrchestrationService;

        public MetricsController(
            IMetricService metricService,
            IMetricOrchestrationService metricOrchestrationService)
        {
            this.metricService = metricService;
            this.metricOrchestrationService = metricOrchestrationService;
        }

        [InvisibleApi]
        [HttpPost]
        public async ValueTask<ActionResult<Metric>> PostMetricAsync([FromBody] Metric metric)
        {
            try
            {
                Metric addedMetric =
                    await this.metricService.AddMetricAsync(metric);

                return Created(addedMetric);
            }
            catch (MetricServiceValidationException metricServiceValidationException)
            {
                return BadRequest(metricServiceValidationException.InnerException);
            }
            catch (MetricServiceDependencyValidationException metricServiceDependencyValidationException)
                when (metricServiceDependencyValidationException.InnerException
                    is AlreadyExistsMetricServiceException)
            {
                return Conflict(metricServiceDependencyValidationException.InnerException);
            }
            catch (MetricServiceDependencyValidationException metricServiceDependencyValidationException)
            {
                return BadRequest(metricServiceDependencyValidationException.InnerException);
            }
            catch (MetricServiceDependencyException metricServiceDependencyException)
            {
                return InternalServerError(metricServiceDependencyException);
            }
            catch (MetricServiceException metricServiceException)
            {
                return InternalServerError(metricServiceException);
            }
        }

        [HttpGet]
#if !DEBUG
        [EnableQuery(PageSize = 50)]
#endif
#if DEBUG
        [EnableQuery(PageSize = 5000)]
#endif
        public async ValueTask<ActionResult<IQueryable<Metric>>> Get()
        {
            try
            {
                IQueryable<Metric> retrievedMetrics =
                    await this.metricService.RetrieveAllMetricsAsync();

                return Ok(retrievedMetrics);
            }
            catch (MetricServiceDependencyException metricServiceDependencyException)
            {
                return InternalServerError(metricServiceDependencyException);
            }
            catch (MetricServiceException metricServiceException)
            {
                return InternalServerError(metricServiceException);
            }
        }

        /// <summary>
        /// Every request matching the filter as a CSV file, unpaged - the export behind the
        /// portal's master list, which otherwise only ever holds the pages scrolled so far. The
        /// filter matches the list's: an optional correlation id, user id and status, and
        /// CreatedDate bounds that are inclusive at both ends. The status binds by name
        /// (status=Failed) or by ordinal.
        ///
        /// A literal segment, so it is matched ahead of the {metricId} route below.
        /// </summary>
        [HttpGet("exports")]
        public async ValueTask<ActionResult> GetMetricExportAsync(
            [FromQuery] Guid? correlationId,
            [FromQuery] string? userId,
            [FromQuery] MetricStatus? status,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate)
        {
            try
            {
                Stream csvStream =
                    await this.metricOrchestrationService.ExportRequestMetricsToCsvAsync(
                        correlationId,
                        userId,
                        status,
                        fromDate,
                        toDate,
                        HttpContext?.RequestAborted ?? default);

                return File(csvStream, contentType: "text/csv", fileDownloadName: "metrics.csv");
            }
            catch (MetricOrchestrationDependencyValidationException
                metricOrchestrationDependencyValidationException)
            {
                return BadRequest(metricOrchestrationDependencyValidationException.InnerException);
            }
            catch (MetricOrchestrationDependencyException metricOrchestrationDependencyException)
            {
                return InternalServerError(metricOrchestrationDependencyException);
            }
            catch (MetricOrchestrationServiceException metricOrchestrationServiceException)
            {
                return InternalServerError(metricOrchestrationServiceException);
            }
        }

        [HttpGet("{metricId}")]
        public async ValueTask<ActionResult<Metric>> GetMetricByIdAsync(Guid metricId)
        {
            try
            {
                Metric metric = await this.metricService.RetrieveMetricByIdAsync(metricId);

                return Ok(metric);
            }
            catch (MetricServiceValidationException metricServiceValidationException)
                when (metricServiceValidationException.InnerException is NotFoundMetricServiceException)
            {
                return NotFound(metricServiceValidationException.InnerException);
            }
            catch (MetricServiceValidationException metricServiceValidationException)
            {
                return BadRequest(metricServiceValidationException.InnerException);
            }
            catch (MetricServiceDependencyValidationException metricServiceDependencyValidationException)
            {
                return BadRequest(metricServiceDependencyValidationException.InnerException);
            }
            catch (MetricServiceDependencyException metricServiceDependencyException)
            {
                return InternalServerError(metricServiceDependencyException);
            }
            catch (MetricServiceException metricServiceException)
            {
                return InternalServerError(metricServiceException);
            }
        }

        [InvisibleApi]
        [HttpDelete("{metricId}")]
        public async ValueTask<ActionResult<Metric>> DeleteMetricByIdAsync(Guid metricId)
        {
            try
            {
                Metric deletedMetric =
                    await this.metricService.RemoveMetricByIdAsync(metricId);

                return Ok(deletedMetric);
            }
            catch (MetricServiceValidationException metricServiceValidationException)
                when (metricServiceValidationException.InnerException is NotFoundMetricServiceException)
            {
                return NotFound(metricServiceValidationException.InnerException);
            }
            catch (MetricServiceValidationException metricServiceValidationException)
            {
                return BadRequest(metricServiceValidationException.InnerException);
            }
            catch (MetricServiceDependencyValidationException metricServiceDependencyValidationException)
                when (metricServiceDependencyValidationException.InnerException
                    is LockedMetricServiceException)
            {
                return Locked(metricServiceDependencyValidationException.InnerException);
            }
            catch (MetricServiceDependencyValidationException metricServiceDependencyValidationException)
            {
                return BadRequest(metricServiceDependencyValidationException.InnerException);
            }
            catch (MetricServiceDependencyException metricServiceDependencyException)
            {
                return InternalServerError(metricServiceDependencyException);
            }
            catch (MetricServiceException metricServiceException)
            {
                return InternalServerError(metricServiceException);
            }
        }
    }
}
