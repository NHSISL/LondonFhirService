// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Formatters
{
    public class FhirJsonOutputFormatter : TextOutputFormatter
    {
        private readonly FhirJsonSerializer fhirJsonSerializer;

        public FhirJsonOutputFormatter()
        {
            this.fhirJsonSerializer = new FhirJsonSerializer();

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/fhir+json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(Resource).IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(
            OutputFormatterWriteContext context,
            Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var resource = context.Object as Resource;

            if (resource != null)
            {
                string jsonContent = this.fhirJsonSerializer.SerializeToString(resource);
                await response.WriteAsync(jsonContent, selectedEncoding);
            }

            await Task.CompletedTask;
        }
    }
}
