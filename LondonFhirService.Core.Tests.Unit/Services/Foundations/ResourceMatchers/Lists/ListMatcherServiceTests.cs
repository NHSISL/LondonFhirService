// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists;

public partial class ListMatcherServiceTests
{
    private readonly ListMatcherService listMatcherService;

    public ListMatcherServiceTests() =>
        this.listMatcherService = new ListMatcherService();

    private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
        actualException => actualException.SameExceptionAs(expectedException);

    private static Dictionary<string, JsonElement> CreateResourceIndex() => new();

    private static JsonElement CreateListResource(
        string title,
        string id = "list-1")
    {
        string json = $$"""
        {
          "resourceType": "List",
          "id": "{{id}}",
          "title": "{{title}}"
        }
        """;

        return ParseJsonElement(json);
    }

    private static JsonElement CreateListResourceWithoutTitle()
    {
        string json = """
        {
          "resourceType": "List",
          "id": "list-1"
        }
        """;

        return ParseJsonElement(json);
    }

    private static JsonElement CreateMalformedTitleResource()
    {
        string json = """
        {
          "resourceType": "List",
          "id": "list-1",
          "title": {
            "value": "Problems"
          }
        }
        """;

        return ParseJsonElement(json);
    }

    private static JsonElement ParseJsonElement(string json) =>
        JsonDocument.Parse(json).RootElement.Clone();
}