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
        public void GetShouldHaveRoleAttributeWithRoles()
        {
            // Given
            var controllerType = typeof(ProvidersController);
            var methodInfo = controllerType.GetMethod("GetProviderByIdAsync");
            Type attributeType = typeof(AuthorizeAttribute);
            string attributeProperty = "Roles";

            List<string> expectedAttributeValues = new List<string>
            {
                "ManageAdmin",
                "ManageUsers"
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
        public void GetShouldNotHaveInvisibleApiAttribute()
        {
            // Given
            var controllerType = typeof(ProvidersController);
            var methodInfo = controllerType.GetMethod("GetProviderByIdAsync");
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
