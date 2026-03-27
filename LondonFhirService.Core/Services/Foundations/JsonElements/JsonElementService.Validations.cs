// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions;

namespace LondonFhirService.Core.Services.Foundations.JsonElements
{
    public partial class JsonElementService
    {
        private void ValidateOnCreateStringElement(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidJsonElementServiceException(
                    message: "String value cannot be null or whitespace.");
            }
        }

        private void ValidateOnCreateArrayElement(List<JsonElement> elements)
        {
            if (elements is null)
            {
                throw new InvalidJsonElementServiceException(
                    message: "Elements list cannot be null.");
            }
        }

        private void ValidateOnCreateObjectElement(Dictionary<string, JsonElement> properties)
        {
            if (properties is null)
            {
                throw new InvalidJsonElementServiceException(
                    message: "Properties dictionary cannot be null.");
            }
        }
    }
}
