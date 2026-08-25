// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Attrify.Attributes;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using LondonFhirService.Core.Services.Foundations.Metrics;
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
    [Authorize(Roles = "Administrators,Users")]
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : RESTFulController
    {
        private readonly IMetricService metricService;

        public MetricsController(IMetricService metricService) =>
            this.metricService = metricService;

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
