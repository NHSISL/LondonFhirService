// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using FluentAssertions;
using LondonFhirService.Core;
using LondonFhirService.Manage.Controllers;
using LondonFhirService.Manage.Models.Versions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Versions
{
    public class VersionsControllerTests
    {
        [Fact]
        public void ShouldReturnTheCoreVersionOnGetVersion()
        {
            // given
            var versionsController = new VersionsController();

            // when
            ActionResult<ApplicationVersion> actualActionResult = versionsController.GetVersion();

            // then
            ApplicationVersion applicationVersion = actualActionResult.Result.Should()
                .BeOfType<OkObjectResult>().Which.Value.Should()
                .BeOfType<ApplicationVersion>().Subject;

            applicationVersion.CoreVersion.Should().Be(CoreVersion.Value);

            // The csproj Version, not a placeholder: a release bump has to show up here.
            applicationVersion.CoreVersion.Should().MatchRegex(@"^\d+\.\d+\.\d+\.\d+$");
        }

        [Fact]
        public void ShouldHaveRoleAttributeWithRoles()
        {
            // given . when
            AuthorizeAttribute authorizeAttribute = typeof(VersionsController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()
                .Single();

            // then
            authorizeAttribute.Roles.Split(',').Select(role => role.Trim()).Should()
                .BeEquivalentTo("Administrators", "Users");
        }
    }
}
