// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Foundations.JsonElements
{
    public partial class JsonElementService : IJsonElementService
    {
        public ValueTask<JsonElement> CreateStringElement(string value) =>
        TryCatch(async () =>
        {
            ValidateOnCreateStringElement(value);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStringValue(value);
            writer.Flush();
            stream.Position = 0;
            using var doc = JsonDocument.Parse(stream);

            return doc.RootElement.Clone();
        });

        public ValueTask<JsonElement> CreateArrayElement(List<JsonElement> elements) =>
        TryCatch(async () =>
        {
            ValidateOnCreateArrayElement(elements);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartArray();

            foreach (var element in elements)
            {
                element.WriteTo(writer);
            }

            writer.WriteEndArray();
            writer.Flush();
            stream.Position = 0;
            using var doc = JsonDocument.Parse(stream);

            return doc.RootElement.Clone();
        });

        public ValueTask<JsonElement> CreateObjectElement(Dictionary<string, JsonElement> properties) =>
        TryCatch(async () =>
        {
            ValidateOnCreateObjectElement(properties);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            foreach (var prop in properties)
            {
                writer.WritePropertyName(prop.Key);
                prop.Value.WriteTo(writer);
            }

            writer.WriteEndObject();
            writer.Flush();
            stream.Position = 0;
            using var doc = JsonDocument.Parse(stream);

            return doc.RootElement.Clone();
        });
    }
}
