// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
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

        public override ValueTask<string> GetMatchKeyAsync(
            JsonElement resource,
            Dictionary<string, JsonElement> resourceIndex) =>
            throw new System.NotImplementedException();

        public override ValueTask<ResourceMatch> MatchAsync(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex) =>
            new ValueTask<ResourceMatch>(new ResourceMatch());
    }
}
