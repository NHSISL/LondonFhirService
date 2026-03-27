// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.EpisodeOfCares.Exceptions
{
    public class EpisodeOfCareMatcherServiceValidationException : Xeption
    {
        public EpisodeOfCareMatcherServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
