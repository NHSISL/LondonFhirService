// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Attrify.Attributes;
using FluentAssertions;
using LondonFhirService.Manage.Controllers.FhirRecords;
using LondonFhirService.Manage.Models.Securities;
using Microsoft.AspNetCore.Authorization;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.FhirRecords
{
    public partial class FhirRecordsControllerTests
    {
        [Fact]
        public void GetAllShouldHaveRoleAttributeWithRoles()
        {
            // Given
            var controllerType = typeof(FhirRecordsController);
            var methodInfo = controllerType.GetMethod("Get");
            Type attributeType = typeof(AuthorizeAttribute);
            string attributeProperty = "Roles";

            // Derived from ManageRoles rather than spelled out, so the assertion is about which
            // audience guards this endpoint and not about how that audience is currently spelled.
            // The literal list this replaced pinned three app registration aliases per role, so
            // when the registration was reduced to one name each every one of these tests failed
            // without a single [Authorize] having changed.
            List<string> expectedAttributeValues = (ManageRoles.AdministratorsAndUsers + ",FhirRecords.Read")
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
                .Where(role => !string.IsNullOrEmpty(role))
                .ToList();

            actualAttributeValues.Should().BeEquivalentTo(expectedAttributeValues);
        }

        [Fact]
        public void GetAllShouldNotHaveInvisibleApiAttribute()
        {
            // Given
            var controllerType = typeof(FhirRecordsController);
            var methodInfo = controllerType.GetMethod("Get");
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
        //public void GetAllShouldHaveInvisibleApiAttribute()
        //{
        //    // Given
        //    var controllerType = typeof(FhirRecordsController);
        //    var methodInfo = controllerType.GetMethod("Get");
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
