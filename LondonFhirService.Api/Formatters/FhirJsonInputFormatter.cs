// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace LondonFhirService.Api.Formatters
{
    public class FhirJsonInputFormatter : TextInputFormatter
    {
        private readonly FhirJsonParser fhirJsonParser;

        public FhirJsonInputFormatter()
        {
            this.fhirJsonParser = new FhirJsonParser();

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
                    Resource resource = this.fhirJsonParser.Parse<Resource>(content);

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
