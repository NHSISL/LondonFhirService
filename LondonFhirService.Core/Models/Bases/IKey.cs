// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Core.Models.Bases
{
    public interface IKey
    {
        Guid Id { get; set; }
    }
}
