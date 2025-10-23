// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Core.Models.Bases
{
    public interface IAudit
    {
        string CreatedBy { get; set; }
        DateTimeOffset CreatedDate { get; set; }
        string UpdatedBy { get; set; }
        DateTimeOffset UpdatedDate { get; set; }
    }
}
