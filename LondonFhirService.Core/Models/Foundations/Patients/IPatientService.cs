// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Core.Models.Foundations.Patients
{
    public interface IPatientService
    {
        ValueTask<List<Bundle>> Everything(
            List<string> providerNames,
            string nhsNumber,
            CancellationToken cancellationToken,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null);
    }
}
