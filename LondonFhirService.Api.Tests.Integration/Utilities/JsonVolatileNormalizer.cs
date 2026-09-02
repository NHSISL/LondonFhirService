// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace LondonFhirService.Api.Tests.Integration.Utilities
{
    public static class JsonVolatileNormalizer
    {
        // GUID and reference GUID patterns
        private static readonly Regex GuidRegex =
            new(@"^[{(]?[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}[)}]?$",
                RegexOptions.Compiled);

        private static readonly Regex ReferenceWithGuidRegex =
            new(@"^(?<type>[A-Za-z][A-Za-z0-9]*)/(?<id>[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12})$",
                RegexOptions.Compiled);

        // ISO-ish timestamps like 2026-01-22T16:35:34+00:00 or 2026-01-22T16:35:34.189+00:00
        private static readonly Regex IsoDateTimeRegex =
            new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?([Zz]|[+\-]\d{2}:\d{2})$",
                RegexOptions.Compiled);

        // Keys that are volatile in your payloads
        private static readonly HashSet<string> VolatileKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "id",           // resource ids
            "lastUpdated",  // meta.lastUpdated
            "date"          // List.date (and possibly other resources)
        };

        public static string Normalize(string json)
        {
            JsonNode? root = JsonNode.Parse(json);
            if (root is null) return string.Empty;

            JsonNode normalized = NormalizeNode(root);

            // Deterministic formatting
            return normalized.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private static JsonNode NormalizeNode(JsonNode node)
        {
            return node switch
            {
                JsonObject obj => NormalizeObject(obj),
                JsonArray arr => NormalizeArray(arr),
                JsonValue val => NormalizeValue(val),
                _ => node.DeepClone()
            };
        }

        private static JsonNode NormalizeObject(JsonObject obj)
        {
            // Create a new object with sorted keys to get stable output
            var sorted = new JsonObject();

            foreach (var kvp in obj.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                string key = kvp.Key;
                JsonNode? value = kvp.Value;

                if (value is null)
                {
                    sorted[key] = null;
                    continue;
                }

                // If it's a known volatile key, replace with placeholder
                if (VolatileKeys.Contains(key))
                {
                    // You can also choose to REMOVE these instead of replacing:
                    // continue;

                    sorted[key] = key.Equals("date", StringComparison.OrdinalIgnoreCase) ||
                                  key.Equals("lastUpdated", StringComparison.OrdinalIgnoreCase)
                        ? "<datetime>"
                        : "<id>";

                    continue;
                }

                // Special-case: "reference": "Patient/{guid}" etc.
                if (key.Equals("reference", StringComparison.OrdinalIgnoreCase) &&
                    value is JsonValue referenceVal &&
                    referenceVal.TryGetValue<string>(out var referenceStr))
                {
                    var m = ReferenceWithGuidRegex.Match(referenceStr);
                    if (m.Success)
                    {
                        sorted[key] = $"{m.Groups["type"].Value}/<id>";
                        continue;
                    }
                }

                sorted[key] = NormalizeNode(value);
            }

            return sorted;
        }

        private static JsonNode NormalizeArray(JsonArray arr)
        {
            var newArr = new JsonArray();
            foreach (var item in arr)
            {
                newArr.Add(item is null ? null : NormalizeNode(item));
            }

            // NOTE:
            // If you ever find arrays whose order is not stable but should be treated as sets,
            // you can sort them here (e.g., by "resourceType"+"<id>" etc.).
            return newArr;
        }

        private static JsonNode NormalizeValue(JsonValue val)
        {
            if (val.TryGetValue<string>(out var s))
            {
                // Replace bare GUID values
                if (GuidRegex.IsMatch(s))
                    return JsonValue.Create("<id>")!;

                // Replace datetime strings (optional – you can restrict to only known keys instead)
                if (IsoDateTimeRegex.IsMatch(s))
                    return JsonValue.Create("<datetime>")!;

                // Replace embedded references if they occur as plain strings elsewhere
                var m = ReferenceWithGuidRegex.Match(s);
                if (m.Success)
                    return JsonValue.Create($"{m.Groups["type"].Value}/<id>")!;
            }

            return val.DeepClone();
        }
    }
}
