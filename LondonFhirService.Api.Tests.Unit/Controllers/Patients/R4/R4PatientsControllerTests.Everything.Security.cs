// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Attrify.Attributes;
//using FluentAssertions;
//using LondonFhirService.Api.Controllers.R4;
//using Microsoft.AspNetCore.Authorization;

//namespace LondonFhirService.Api.Tests.Unit.Controllers.Patients.R4
//{
//    public partial class R4PatientControllerTests
//    {
//        [Fact]
//        public void GetShouldHaveRolesAttribute()
//        {
//            // given
//            var controllerType = typeof(PatientController);
//            var methodInfo = controllerType.GetMethod("Everything");
//            Type attributeType = typeof(AuthorizeAttribute);
//            string attributeProperty = "Roles";

//            List<string> expectedAttributeValues = new List<string>
//            {
//                "Patients.Everything"
//            };

//            // when
//            var methodAttribute = methodInfo?
//                .GetCustomAttributes(attributeType, inherit: true)
//                .FirstOrDefault();

//            var controllerAttribute = controllerType
//                .GetCustomAttributes(attributeType, inherit: true)
//                .FirstOrDefault();

//            var attribute = methodAttribute ?? controllerAttribute;

//            // then
//            attribute.Should().NotBeNull();

//            var actualAttributeValue = attributeType
//                .GetProperty(attributeProperty)?
//                .GetValue(attribute) as string ?? string.Empty;

//            var actualAttributeValues = actualAttributeValue?
//                .Split(',')
//                .Select(role => role.Trim())
//                .Where(role => !string.IsNullOrEmpty(role))
//                .ToList();

//            actualAttributeValues.Should().BeEquivalentTo(expectedAttributeValues);
//        }

//        [Fact]
//        public void GetShouldNotHaveInvisibleApiAttribute()
//        {
//            // Given
//            var controllerType = typeof(PatientController);
//            var methodInfo = controllerType.GetMethod("Everything");
//            Type attributeType = typeof(InvisibleApiAttribute);

//            // When
//            var methodAttribute = methodInfo?
//                .GetCustomAttributes(attributeType, inherit: true)
//                .FirstOrDefault();

//            var controllerAttribute = controllerType
//                .GetCustomAttributes(attributeType, inherit: true)
//                .FirstOrDefault();

//            var attribute = methodAttribute ?? controllerAttribute;

//            // Then
//            attribute.Should().BeNull();
//        }
//    }
//}
