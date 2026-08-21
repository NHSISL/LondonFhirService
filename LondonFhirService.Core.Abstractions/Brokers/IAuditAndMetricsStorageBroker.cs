// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Abstractions.Brokers
{
    /// <summary>
    /// The persistence the audit and metrics library needs, declared here rather than consumed
    /// from the hosting application. The application supplies an implementation, so the
    /// dependency runs one way - application to library - and the reference stays acyclic while
    /// the library still writes to the application's database.
    ///
    /// Everything is expressed in terms of IMetric and IAudit; the library never sees the
    /// concrete entities or the ORM behind them.
    ///
    /// Implementations are also responsible for classifying storage failures. The library carries
    /// no ORM or database driver, so it cannot name SqlException or DbUpdateException - an
    /// implementation catches those and re-throws the storage exceptions in
    /// Models.Metrics.Exceptions and Models.Audits.Exceptions, which are the contract between the
    /// two. Cancellation and timeout must pass through untranslated; the library handles those.
    /// </summary>
    public partial interface IAuditAndMetricsStorageBroker
    { }
}
