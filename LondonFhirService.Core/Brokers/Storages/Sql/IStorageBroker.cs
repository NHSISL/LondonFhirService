// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Abstractions.Brokers;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    /// <summary>
    /// Inherits the audit and metrics storage port, so the audit and metric members are declared
    /// once, in Core.Abstractions, over IAudit and IMetric. There are no separate
    /// IStorageBroker.Audit.cs or IStorageBroker.Metric.cs partials - those members come from the
    /// port, which is what lets the standalone library share this broker without Core having to
    /// hand it a concrete type.
    /// </summary>
    public partial interface IStorageBroker : IAsyncDisposable, IAuditAndMetricsStorageBroker
    { }
}
