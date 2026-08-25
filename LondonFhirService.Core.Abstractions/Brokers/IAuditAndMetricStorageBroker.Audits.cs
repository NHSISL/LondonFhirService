// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Audits;

namespace LondonFhirService.Core.Abstractions.Brokers
{
    public partial interface IAuditAndMetricStorageBroker
    {
        /// <summary>
        /// A new, empty audit entry for the library to populate. The library holds only the
        /// contract, so it cannot construct one - and it must not, because the concrete type is
        /// whatever the hosting application maps to its database.
        /// </summary>
        IAudit CreateAudit();

        ValueTask<IAudit> InsertAuditAsync(IAudit audit, CancellationToken cancellationToken = default);
        ValueTask BulkInsertAuditsAsync(List<IAudit> audits, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<IAudit>> SelectAllAuditsAsync(CancellationToken cancellationToken = default);
        ValueTask<IAudit> SelectAuditByIdAsync(Guid auditId, CancellationToken cancellationToken = default);
        ValueTask<IAudit> UpdateAuditAsync(IAudit audit, CancellationToken cancellationToken = default);
        ValueTask<IAudit> DeleteAuditAsync(IAudit audit, CancellationToken cancellationToken = default);
    }
}
