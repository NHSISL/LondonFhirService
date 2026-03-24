// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Attrify.Attributes;
using FluentAssertions;
using LondonFhirService.Manage.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecordDifferences
{
    public partial class FhirRecordDifferencesControllerTests
    {
        [Fact]
        public void PutShouldHaveRoleAttributeWithRoles()
        {
            // Given
            var controllerType = typeof(FhirRecordDifferencesController);
            var methodInfo = controllerType.GetMethod("PutFhirRecordDifferenceAsync");
            Type attributeType = typeof(AuthorizeAttribute);
            string attributeProperty = "Roles";

            List<string> expectedAttributeValues = new List<string>
            {
                "LondonFhirService.Manage.Administrators",
                "FhirRecordDifferences.Update"
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
        public void PutShouldNotHaveInvisibleApiAttribute()
        {
            // Given
            var controllerType = typeof(FhirRecordDifferencesController);
            var methodInfo = controllerType.GetMethod("PutFhirRecordDifferenceAsync");
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

        //[Fact]
        //public void PutShouldHaveInvisibleApiAttribute()
        //{
        //    // Given
        //    var controllerType = typeof(FhirRecordDifferencesController);
        //    var methodInfo = controllerType.GetMethod("PutFhirRecordDifferenceAsync");
        //    Type attributeType = typeof(InvisibleApiAttribute);
        //
        //    // When
        //    var methodAttribute = methodInfo?
        //        .GetCustomAttributes(attributeType, inherit: true)
        //        .FirstOrDefault();
        //
        //    var controllerAttribute = controllerType
        //        .GetCustomAttributes(attributeType, inherit: true)
        //        .FirstOrDefault();
        //
        //    var attribute = methodAttribute ?? controllerAttribute;
        //
        //    // Then
        //    attribute.Should().NotBeNull();
        //}
    }
}
