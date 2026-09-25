// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core;
using LondonFhirService.Manage.Models.Securities;
using LondonFhirService.Manage.Models.Versions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTFulSense.Controllers;

namespace LondonFhirService.Manage.Controllers
{
    /// <summary>
    /// The release this portal is running, shown in its header. The same value the Api host
    /// reports on its root route: LondonFhirService.Core's assembly version.
    ///
    /// Behind the same roles as the rest of this host. The header only renders for a signed in
    /// user, so nothing needs to read this anonymously.
    /// </summary>
    [Authorize(Roles = ManageRoles.AdministratorsAndUsers)]
    [ApiController]
    [Route("api/[controller]")]
    public class VersionsController : RESTFulController
    {
        [HttpGet]
        public ActionResult<ApplicationVersion> GetVersion() =>
            Ok(new ApplicationVersion { CoreVersion = CoreVersion.Value });
    }
}
