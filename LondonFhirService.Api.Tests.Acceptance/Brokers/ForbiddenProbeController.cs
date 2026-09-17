// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    /// <summary>
    /// An endpoint the acceptance principal is guaranteed to fail, so a run can exercise a
    /// response the authorization middleware produces itself.
    ///
    /// Every other pre-controller case the suite covers - an unmatched URL, an unhandled
    /// exception - is produced by routing or by the exception handler. Neither proves the thing
    /// the correlation middleware's placement is for: that it runs BEFORE authentication,
    /// authorization and request timeouts, so the responses those short-circuit with carry the
    /// header too. Move UseMiddleware&lt;CorrelationMiddleware&gt; below UseAuthorization and every
    /// other test still passes; this one does not.
    ///
    /// The role is deliberately one no principal holds. TestAuthHandler issues
    /// Patients.Everything, Patients.GetStructuredRecord and Administrators, so authentication
    /// succeeds and authorization is what refuses - a 403 from the middleware rather than a 401
    /// from the handler or a 404 from routing.
    /// </summary>
    [ApiController]
    [Route("test-only/forbidden")]
    [Authorize(Roles = RequiredRole)]
    public class ForbiddenProbeController : ControllerBase
    {
        public const string Route = "test-only/forbidden";

        private const string RequiredRole = "ARoleNoAcceptancePrincipalHolds";

        [HttpGet]
        public IActionResult Get() =>
            this.Ok();
    }
}
