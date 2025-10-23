// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using LondonFhirService.Core.Models.Bases;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LondonFhirService.Core.Models.Foundations.Consumers
{
    public class Consumer : IKey, IAudit
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactNotes { get; set; }
        public DateTimeOffset ActiveFrom { get; set; }
        public DateTimeOffset? ActiveTo { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }

        [BindNever]
        public List<ConsumerAccess> ConsumerAccesses { get; set; } = new List<ConsumerAccess>();
    }
}
