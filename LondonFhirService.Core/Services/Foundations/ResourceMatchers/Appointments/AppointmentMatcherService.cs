// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Appointments
{
    public partial class AppointmentMatcherService : ResourceMatcherServiceBase, IResourceMatcherService
    {
        public AppointmentMatcherService(ILoggingBroker loggingBroker)
            : base(loggingBroker)
        { }

        public override string ResourceType => "Appointment";

        private const string DdsIdentifierSystem = "https://fhir.hl7.org.uk/Id/dds";

        public override ValueTask<string> GetMatchKeyAsync(
            JsonElement resource,
            Dictionary<string, JsonElement> resourceIndex) =>
            throw new System.NotImplementedException();

        public override ValueTask<ResourceMatch> MatchAsync(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex)
        {
            var resourceMatch = new ResourceMatch();

            var source1ByKey = source1Resources
                .Select(r => new { Resource = r, Key = InternalGetMatchKey(r) })
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key!, x => x.Resource);

            var source2ByKey = source2Resources
                .Select(r => new { Resource = r, Key = InternalGetMatchKey(r) })
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key!, x => x.Resource);

            foreach (var key in source1ByKey.Keys)
            {
                if (source2ByKey.ContainsKey(key))
                {
                    resourceMatch.Matched.Add(
                        new MatchedResource(source1ByKey[key], source2ByKey[key], key));
                }
            }

            return new ValueTask<ResourceMatch>(resourceMatch);
        }

        internal virtual string InternalGetMatchKey(JsonElement resource)
        {
            if (!resource.TryGetProperty("identifier", out var identifiers))
                return null;

            foreach (var identifier in identifiers.EnumerateArray())
            {
                if (!identifier.TryGetProperty("system", out var system))
                    continue;

                if (system.GetString() == DdsIdentifierSystem)
                {
                    if (identifier.TryGetProperty("value", out var value))
                        return value.GetString();
                }
            }

            return null;
        }
    }
}
