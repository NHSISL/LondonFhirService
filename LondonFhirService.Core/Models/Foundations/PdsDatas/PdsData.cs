// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas
{
    public class PdsData
    {
        public Guid Id { get; set; }
        public string NhsNumber { get; set; }
        public string OrgCode { get; set; }
        public string OrganisationName { get; set; }
        public DateTimeOffset? RelationshipWithOrganisationEffectiveFromDate { get; set; }
        public DateTimeOffset? RelationshipWithOrganisationEffectiveToDate { get; set; }
    }
}
