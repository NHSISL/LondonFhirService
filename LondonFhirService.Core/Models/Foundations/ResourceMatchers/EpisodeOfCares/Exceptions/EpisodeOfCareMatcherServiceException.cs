// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.EpisodeOfCares.Exceptions
{
    public class EpisodeOfCareMatcherServiceException : Xeption
    {
        public EpisodeOfCareMatcherServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
