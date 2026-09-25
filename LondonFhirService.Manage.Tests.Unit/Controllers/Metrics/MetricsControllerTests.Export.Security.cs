// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Attrify.Attributes;
using FluentAssertions;
using LondonFhirService.Manage.Controllers.Metrics;
using Microsoft.AspNetCore.Authorization;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    public partial class MetricsControllerTests
    {
        [Fact]
        public void GetMetricExportShouldHaveRoleAttributeWithRoles()
        {
            // Given
            var controllerType = typeof(MetricsController);
            var methodInfo = controllerType.GetMethod(nameof(MetricsController.GetMetricExportAsync));
            Type attributeType = typeof(AuthorizeAttribute);

            List<string> expectedAttributeValues = new List<string>
            {
                "Administrators",
                "Users"
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

            var actualAttributeValues = ((attribute as AuthorizeAttribute)?.Roles ?? string.Empty)
                .Split(',')
                .Select(role => role.Trim())
                .Where(role => !string.IsNullOrEmpty(role))
                .ToList();

            actualAttributeValues.Should().BeEquivalentTo(expectedAttributeValues);
        }

        /// <summary>
        /// The export is an operator-facing read like the list it backs, not a seeding hook for
        /// the acceptance suite, so it must be routable without the invisible-api key.
        /// </summary>
        [Fact]
        public void GetMetricExportShouldNotHaveInvisibleApiAttribute()
        {
            // Given
            var controllerType = typeof(MetricsController);
            var methodInfo = controllerType.GetMethod(nameof(MetricsController.GetMetricExportAsync));
            Type attributeType = typeof(InvisibleApiAttribute);

            // When
            var methodAttribute = methodInfo?
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            var controllerAttribute = controllerType
                .GetCustomAttributes(attributeType, inherit: true)
                .FirstOrDefault();

            // Then
            (methodAttribute ?? controllerAttribute).Should().BeNull();
        }
    }
}
