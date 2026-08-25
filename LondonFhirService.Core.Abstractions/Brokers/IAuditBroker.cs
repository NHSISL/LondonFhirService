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
    /// <summary>
    /// The audit persistence the audit and metrics library needs, declared here rather than
    /// consumed from the hosting application. The application supplies an implementation, so the
    /// dependency runs one way - application to library - and the reference stays acyclic while
    /// the library still writes to the application's database.
    ///
    /// Everything is expressed in terms of IAudit; the library never sees the concrete entity or
    /// the ORM behind it.
    ///
    /// Audits and metrics are separate ports because they are separate entities. A single port
    /// carrying both forces one implementation to serve two entities, and forces the library's
    /// MetricService to take a dependency that can also write audits.
    ///
    /// Implementations are also responsible for classifying storage failures. The library carries
    /// no ORM or database driver, so it cannot name SqlException or DbUpdateException - an
    /// implementation catches those and re-throws the storage exceptions in
    /// Models.Audits.Exceptions, which are the contract between the two. Cancellation and timeout
    /// must pass through untranslated; the library handles those.
    /// </summary>
    public interface IAuditBroker
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
