// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Brokers.Storages.Sql;

namespace LondonFhirService.Core.Brokers.AuditAndMetrics
{
    /// <summary>
    /// Satisfies the storage port the audit and metrics library declares.
    ///
    /// It is a service rather than a broker because its job is categorising exceptions, and that
    /// is foundation service work. The storage broker stays a thin pass through and lets raw
    /// SqlException and DbUpdateException escape, exactly as it does for every other entity.
    ///
    /// Writes go through the factory and get their own short lived context. That is what makes
    /// them safe to fire and forget: a write dispatched to the background outlives the request
    /// scope, and using the request's context would fail once that scope is disposed. Reads use
    /// the scoped broker, because they return an IQueryable the caller enumerates and disposing
    /// the context underneath it would kill the query.
    /// </summary>
    public partial class AuditAndMetricStorageBroker : IAuditAndMetricStorageBroker
    {
        private readonly IStorageBrokerFactory storageBrokerFactory;
        private readonly IStorageBroker storageBroker;

        public AuditAndMetricStorageBroker(
            IStorageBrokerFactory storageBrokerFactory,
            IStorageBroker storageBroker)
        {
            this.storageBrokerFactory = storageBrokerFactory;
            this.storageBroker = storageBroker;
        }
    }
}
