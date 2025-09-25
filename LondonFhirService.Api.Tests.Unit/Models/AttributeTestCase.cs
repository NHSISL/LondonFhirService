// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Api.Tests.Unit.Models
{
    public class AttributeTestCase
    {
        public Type ControllerType { get; set; }
        public string MethodName { get; set; }
        public Type AttributeType { get; set; }
        public string AttributeProperty { get; set; }
        public object ExpectedValue { get; set; }
    }
}
