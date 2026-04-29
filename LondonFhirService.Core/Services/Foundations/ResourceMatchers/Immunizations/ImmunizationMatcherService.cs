// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Immunizations
{
    public partial class ImmunizationMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        public ImmunizationMatcherService(ILoggingBroker loggingBroker)
            : base(loggingBroker)
        { }

        public override string ResourceType => "Immunization";
        private const string DdsIdentifierSystem = "https://fhir.hl7.org.uk/Id/dds";

        public override ValueTask<string> GetMatchKeyAsync(
            JsonElement resource, Dictionary<string, JsonElement> resourceIndex) =>
        TryCatch(async () =>
        {
            ValidateOnGetMatchKeyArguments(resource, resourceIndex);

            return InternalGetMatchKey(resource, resourceIndex);
        });

        public override ValueTask<ResourceMatch> MatchAsync(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex) =>
        TryCatch(async () =>
        {
            ValidateOnMatchArguments(source1Resources, source2Resources, source1ResourceIndex, source2ResourceIndex);

            return new ResourceMatch();
        });

        internal virtual string InternalGetMatchKey(JsonElement resource, Dictionary<string, JsonElement> resourceIndex)
        {
            return null;
        }
    }
}
