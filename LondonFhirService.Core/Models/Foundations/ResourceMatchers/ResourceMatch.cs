// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;

namespace LondonFhirService.Core.Models.Foundations.AllergyIntolerances
{
    public class ResourceMatch
    {
        public List<MatchedResource> Matched { get; set; } = new List<MatchedResource>();
        public List<UnmatchedResource> Unmatched { get; set; } = new List<UnmatchedResource>();
    }
}
