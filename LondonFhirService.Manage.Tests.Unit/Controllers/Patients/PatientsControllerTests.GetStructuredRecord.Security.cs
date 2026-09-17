// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Attrify.Attributes;
using FluentAssertions;
using LondonFhirService.Manage.Controllers.Patients;
using LondonFhirService.Manage.Models.Securities;
using Microsoft.AspNetCore.Authorization;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Patients
{
    public partial class PatientsControllerTests
    {
        /// <summary>
        /// The endpoint hands back a whole patient record for whatever NHS number is typed, so the
        /// guard is asserted against ManageRoles.AdministratorsAndUsers rather than against a
        /// literal list. That constant is the single place this host spells its role names, and
        /// pinning the strings here would only restate what it already says while going stale the
        /// next time an alias is retired from the app registration.
        /// </summary>
        [Fact]
        public void PostGetStructuredRecordShouldHaveRoleAttributeWithAdministratorsAndUsersRoles()
        {
            // given
            var controllerType = typeof(PatientsController);
            var methodInfo = controllerType.GetMethod("PostGetStructuredRecordAsync");
            Type attributeType = typeof(AuthorizeAttribute);
            string attributeProperty = "Roles";

            List<string> expectedAttributeValues = ManageRoles.AdministratorsAndUsers
                .Split(',')
                .Select(role => role.Trim())
                .Where(role => string.IsNullOrEmpty(role) is false)
                .ToList();

            // When
            var methodAttribute = methodInfo?
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            var controllerAttribute = controllerType
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            var attribute = methodAttribute ?? controllerAttribute;

            // Then
            attribute.Should().NotBeNull();

            var actualAttributeValue = attributeType
                .GetProperty(attributeProperty)?
                .GetValue(attribute) as string ?? string.Empty;

            var actualAttributeValues = actualAttributeValue?
                .Split(',')
                .Select(role => role.Trim())
                .Where(role => string.IsNullOrEmpty(role) is false)
                .ToList();

            actualAttributeValues.Should().BeEquivalentTo(expectedAttributeValues);

            // The two names currently assigned in the app registration. Asserted alongside the
            // constant so an accidental narrowing to administrators only, or a rename that drops
            // one of them, fails here rather than at sign in.
            actualAttributeValues.Should().Contain("Administrators");
            actualAttributeValues.Should().Contain("Users");
        }

        [Fact]
        public void PostGetStructuredRecordShouldNotBeAnonymous()
        {
            // given
            var controllerType = typeof(PatientsController);
            var methodInfo = controllerType.GetMethod("PostGetStructuredRecordAsync");
            Type attributeType = typeof(AllowAnonymousAttribute);

            // When
            var methodAttribute = methodInfo?
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            var controllerAttribute = controllerType
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            var attribute = methodAttribute ?? controllerAttribute;

            // Then
            attribute.Should().BeNull();
        }

        /// <summary>
        /// Unlike the seed-and-tear-down verbs on the telemetry controllers, this endpoint is the
        /// feature the page calls, so it must stay routable to a caller who holds no invisible-api
        /// key.
        /// </summary>
        [Fact]
        public void PostGetStructuredRecordShouldNotHaveInvisibleApiAttribute()
        {
            // given
            var controllerType = typeof(PatientsController);
            var methodInfo = controllerType.GetMethod("PostGetStructuredRecordAsync");
            Type attributeType = typeof(InvisibleApiAttribute);

            // When
            var methodAttribute = methodInfo?
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            var controllerAttribute = controllerType
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            var attribute = methodAttribute ?? controllerAttribute;

            // Then
            attribute.Should().BeNull();
        }
    }
}
