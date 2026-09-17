// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Attrify.Attributes;
using FluentAssertions;
using LondonFhirService.Manage.Controllers.Providers;
using Microsoft.AspNetCore.Authorization;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Providers
{
    public partial class ProvidersControllerTests
    {
        [Fact]
        public void PostShouldHaveRoleAttributeWithRoles()
        {
            // Given
            var controllerType = typeof(ProvidersController);
            var methodInfo = controllerType.GetMethod("PostProviderAsync");
            Type attributeType = typeof(AuthorizeAttribute);
            string attributeProperty = "Roles";

            // Narrower than the class attribute on purpose. A provider row decides who the
            // patient fan-out calls and which source is primary, so writing one is an
            // Administrators-only act even though reading the registry is not.
            List<string> expectedAttributeValues = new List<string>
            {
                "Administrators"
            };

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
                .Where(role => !string.IsNullOrEmpty(role))
                .ToList();

            actualAttributeValues.Should().BeEquivalentTo(expectedAttributeValues);
        }

        [Fact]
        public void PostShouldNotHaveInvisibleApiAttribute()
        {
            // Given
            var controllerType = typeof(ProvidersController);
            var methodInfo = controllerType.GetMethod("PostProviderAsync");
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
            // Providers are configuration an operator manages, not a compliance record or a
            // telemetry span only the acceptance suite should seed. Nothing here is hidden.
            attribute.Should().BeNull();
        }
    }
}
