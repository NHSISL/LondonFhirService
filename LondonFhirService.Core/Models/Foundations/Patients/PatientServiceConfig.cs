// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Models.Foundations.Patients
{
    public class PatientServiceConfig
    {
        public int MaxProviderWaitTimeMilliseconds { get; set; }
        public bool IsComparisonServiceActive { get; set; } = true;
    }
}
