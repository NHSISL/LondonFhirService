// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirR4;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FhirR4::Hl7.Fhir.Serialization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using ModelInfo = FhirR4::Hl7.Fhir.Model.ModelInfo;

namespace LondonFhirService.Api.Formatters
{
    public class FhirJsonInputFormatter : TextInputFormatter
    {
        public FhirJsonInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/fhir+json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanReadType(Type type)
        {
            return typeof(Resource).IsAssignableFrom(type);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context,
            Encoding encoding)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            using (var reader = new StreamReader(request.Body, encoding))
            {
                try
                {
                    string content = await reader.ReadToEndAsync();

                    var options = new JsonSerializerOptions()
                        .ForFhir(ModelInfo.ModelInspector);

                    Resource resource = JsonSerializer.Deserialize<Resource>(content, options);

                    return await InputFormatterResult.SuccessAsync(resource);
                }
                catch (Exception)
                {
                    return await InputFormatterResult.FailureAsync();
                }
            }
        }
    }
}
