/* =====================================================================
   London FHIR Service solution dependency data — consumed by index.html
   (both the single-copy and the per-consumer view).

   Hand-maintained model of the solution's components and flows,
   generated from the actual source (2026-08-25). The uniform CRUD
   foundation services and the 21 resource matchers follow templates, so
   they are expanded from the configs below instead of being written out
   by hand.

   Shape:
     projects:   { id, name, kind: internal|library|external }
     components: { id, name, project, layer, col, methods[], utility?,
                   shared?, description? }
        - col: layout column (left → right)
        - utility: hidden unless the "utility brokers" toggle is on
        - shared: consumers link to ONE copy (library/external exposers)
          instead of getting a duplicated copy each
     events:     { id, publish, subscribe }  (row labels on an event broker)
     edges:      direct    { kind:"direct", from:[comp,method|null],
                             to:[comp,method|null] }
                 publish   { kind:"publish", from:[comp,method], event }
                 subscribe { kind:"subscribe", event, to:[comp,handler] }
     roots:      component ids that start a tree (layout order)

   NOTE: this solution has no event bus today — `events` is empty and
   every edge is a direct call (blue). The publish/subscribe machinery is
   left in the renderer so an EventBroker can be modelled later without
   touching index.html.
   ===================================================================== */

(function () {
  const projects = [
    { id: "manage-client", name: "LondonFhirService.Manage.Client", kind: "internal" },
    { id: "manage", name: "LondonFhirService.Manage", kind: "internal" },
    { id: "api", name: "LondonFhirService.Api", kind: "internal" },
    { id: "core", name: "LondonFhirService.Core", kind: "internal" },
    { id: "core-abstractions", name: "LondonFhirService.Core.Abstractions", kind: "internal" },
    { id: "clients-am", name: "LondonFhirService.Clients.AuditAndMetrics", kind: "internal" },
    { id: "pkg-security", name: "ISL.Security.Client", kind: "library" },
    { id: "pkg-efcore", name: "STX.EFCore.Client", kind: "library" },
    { id: "pkg-fhir", name: "LondonFhirService.Providers.FHIR.STU3", kind: "library" },
    { id: "ext-efcore", name: "EF Core / SQL Server", kind: "external" },
    { id: "ext-logging", name: "Microsoft.Extensions.Logging", kind: "external" },
    { id: "ext-appinsights", name: "System.Diagnostics / Application Insights", kind: "external" },
    { id: "ext-azure", name: "Azure.Identity", kind: "external" },
    { id: "ext-http", name: "System.Net.Http", kind: "external" },
    { id: "ext-fhir-src", name: "Upstream STU3 FHIR services", kind: "external" },
  ];

  const components = [];
  const events = [];
  const edges = [];
  const roots = [];

  const C = (comp) => { components.push(comp); return comp.id; };
  const D = (from, to) => edges.push({ kind: "direct", from, to });
  const P = (comp, method, event) => edges.push({ kind: "publish", from: [comp, method], event });
  const S = (event, comp, handler) => edges.push({ kind: "subscribe", event, to: [comp, handler] });

  /* ==================================================================
     Columns:
     0  Manage.Client entry points     1  Manage.Client view services
     2  Manage.Client brokers          3  host exposers (controllers/worker)
     4  Core coordinations             5  Core orchestrations
     6  Core processings               7  Core foundations
     8  Core brokers                   9  Core clients
     10 library exposers               11 library internals
     12 far externals
     ================================================================== */

  /* ==================================================================
     External surfaces (shared, single copy). Their method rows are
     derived from the declared edges further down, so rows and arrows
     can never drift apart.
     ================================================================== */
  C({ id: "EXT.EFCore", name: "DbContext (EF Core / SQL Server)", project: "ext-efcore", layer: "external", col: 12, shared: true, methods: [],
      description: "The DbContext handed to STX.EFCore.Client's EFCoreClient. In this solution that is Core's StorageBroker itself — it derives from EFxceptionsContext (EF Core DbContext) and passes `this` into `new EFCoreClient(this)`. SQL Server with HierarchyId enabled. Three methods bypass EFCoreClient and hit EF directly: ClaimFhirRecordAsync and DeleteMetricsOlderThanAsync run set-based ExecuteUpdate/ExecuteDelete statements, and SelectAllProvidersAsListAsync materialises with ToListAsync." });
  C({ id: "EXT.ILogger", name: "ILogger<LoggingBroker>", project: "ext-logging", layer: "external", col: 12, shared: true, methods: [],
      description: "Microsoft.Extensions.Logging. The only sink LoggingBroker writes to; Application Insights is wired at host level." });
  C({ id: "EXT.ActivitySource", name: "ActivitySource → Application Insights", project: "ext-appinsights", layer: "external", col: 12, shared: true, methods: [],
      description: "System.Diagnostics.ActivitySource. The audit and metrics library publishes every completed span here as a second sink alongside the database. Nothing was subscribed until MetricTelemetryPublisher was added to the API host — every span published before that was dropped before it reached Application Insights." });
  C({ id: "EXT.TokenCredential", name: "TokenCredential (DefaultAzureCredential)", project: "ext-azure", layer: "external", col: 12, shared: true, methods: [],
      description: "Azure.Identity. Registered as a singleton in the API host and used by ConsumerAccessBroker to mint a bearer token for the remote consumer-access endpoint." });
  C({ id: "EXT.HttpClient", name: "HttpClient", project: "ext-http", layer: "external", col: 12, shared: true, methods: [],
      description: "Typed HttpClient registered via AddHttpClient<IConsumerAccessBroker, ConsumerAccessBroker>()." });
  C({ id: "EXT.DdsStu3", name: "DdsStu3Provider → Discovery Data Service", project: "ext-fhir-src", layer: "external", col: 12, shared: true, methods: [],
      description: "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService. One of the two IFhirProvider implementations registered in Program.Configurations.AddProviders." });
  C({ id: "EXT.LdsStu3", name: "LdsStu3Provider → London Data Service", project: "ext-fhir-src", layer: "external", col: 12, shared: true, methods: [],
      description: "LondonFhirService.Providers.FHIR.STU3.LondonDataService. The second registered IFhirProvider; constructed with IHttpContextAccessor so it can forward the caller's identity." });

  /* ==================================================================
     Client libraries (shared, single copy).
     ================================================================== */
  C({ id: "LIB.SecurityClient", name: "SecurityClient", project: "pkg-security", layer: "client", col: 10, shared: true,
      methods: [
        "Audits.ApplyAddAuditValuesAsync",
        "Audits.ApplyModifyAuditValuesAsync",
        "Audits.ApplyRemoveAuditValuesAsync",
        "Audits.EnsureOtherAuditValuesRemainsUnchangedOnModifyAsync",
        "Audits.GetUserIdAsync",
        "Users.GetUserAsync",
        "Users.IsUserAuthenticatedAsync",
        "Users.IsUserInRoleAsync",
        "Users.UserHasClaimAsync",
      ],
      description: "ISL.Security.Client 6.0.0. Core's SecurityBroker and SecurityAuditBroker each `new SecurityClient()` directly (it is not DI-registered) and pass the ClaimsPrincipal taken from IHttpContextAccessor — or parsed from a JWT / supplied directly by the two non-REST constructors." });

  C({ id: "LIB.EFCoreClient", name: "EFCoreClient", project: "pkg-efcore", layer: "client", col: 10, shared: true,
      methods: ["InsertAsync", "SelectAllAsync", "SelectAsync", "UpdateAsync", "DeleteAsync", "BulkInsertAsync", "BulkUpdateAsync", "BulkDeleteAsync"],
      description: "STX.EFCore.Client 3.0.0. Most StorageBroker partials funnel through these eight generic primitives; the broker constructs it with itself as the DbContext." });

  C({ id: "LIB.FhirAbstractionProvider", name: "FhirAbstractionProvider", project: "pkg-fhir", layer: "client", col: 10, shared: true, methods: [],
      description: "LondonFhirService.Providers.FHIR.STU3.Abstractions. Registered as a singleton wrapping the DDS and LDS providers; Stu3FhirBroker forwards its whole typed-resource surface. Method rows are derived from the declared edges." });

  C({ id: "LIB.FhirProvider", name: "IFhirProvider", project: "pkg-fhir", layer: "client", col: 11, shared: true,
      methods: ["ProviderName", "DisplayName", "SupportsResource", "Patients.GetStructuredRecordSerialisedAsync"],
      description: "The per-provider surface Stu3PatientService actually calls, resolved out of the FhirProviders collection by matching Provider.FullyQualifiedName. Two implementations are registered: DdsStu3Provider and LdsStu3Provider." });

  D(["LIB.FhirAbstractionProvider", "FhirProviders"], ["LIB.FhirProvider", null]);
  for (const impl of ["EXT.DdsStu3", "EXT.LdsStu3"])
    D(["LIB.FhirProvider", "Patients.GetStructuredRecordSerialisedAsync"], [impl, "Patients.GetStructuredRecordSerialisedAsync"]);

  /* ==================================================================
     LondonFhirService.Clients.AuditAndMetrics — the audit and metrics
     library, extracted out of Core.

     Recording an audit or a metric is service work, but every layer
     needs to call it and a broker may not call a service. Moving those
     services into a separate library makes Core's AuditAndMetricBroker
     a wrapper over an external dependency, which is what a broker is
     for. The library carries no ORM: persistence arrives through the
     IAuditAndMetricStorageBroker port that Core implements, and
     deferred work is handed back to the host through
     IAuditAndMetricsDispatcher.
     ================================================================== */
  C({ id: "LIB.AMClient", name: "AuditAndMetricsClient", project: "clients-am", layer: "client", col: 10, shared: true,
      methods: ["AuditClient", "MetricClient", "BindConfigurations"],
      description: "Follows the SecurityClient shape: owns its own ServiceCollection, registers everything it needs, builds a provider and resolves its two sub-clients from it. The host hands in the things the library cannot supply itself — somewhere to persist to, who the caller is, its configuration, a logger factory and (optionally) a dispatcher. Registered Scoped in both hosts, and it has to stay that way: SecurityAuditBroker captures the ClaimsPrincipal in its constructor, so a singleton would stamp every audit after the first with the wrong user." });
  D(["LIB.AMClient", "AuditClient"], ["LIB.AM.AuditClient", null]);
  D(["LIB.AMClient", "MetricClient"], ["LIB.AM.MetricClient", null]);

  C({ id: "LIB.AM.AuditClient", name: "AuditClient", project: "clients-am", layer: "client", col: 11,
      methods: ["LogAuditAsync", "RecordAuditAsync", "AddAuditAsync", "BulkLogAuditsAsync", "BulkAddAuditsAsync",
                "RetrieveAllAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync"],
      description: "The outward-facing surface over the audit foundation service. Service exceptions are re-thrown as client exceptions so callers depend on the client's contract rather than the service layer's. A missing audit comes out as AuditClientNotFoundException so a caller can answer 404 without naming the library's internal categorization types. Cancellation is deliberately not translated." });
  for (const m of ["LogAuditAsync", "RecordAuditAsync", "AddAuditAsync", "BulkLogAuditsAsync", "BulkAddAuditsAsync",
                   "RetrieveAllAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync"])
    D(["LIB.AM.AuditClient", m], ["LIB.AM.AuditService", m]);

  C({ id: "LIB.AM.MetricClient", name: "MetricClient", project: "clients-am", layer: "client", col: 11,
      methods: ["AddMetricAsync", "AddMetricsAsync", "LogMetricAsync", "LogMetricsAsync",
                "RetrieveAllMetricsAsync", "RetrieveMetricByIdAsync", "RemoveMetricByIdAsync",
                "PurgeMetricsOlderThanRetentionPeriodAsync"],
      description: "The metric counterpart to AuditClient. Its TryCatch is written once rather than repeated per method. A missing span comes out as MetricClientNotFoundException — before that existed it was folded into MetricClientValidationException, whose inner NotFoundMetricException is internal to this library, so every miss reached callers as a 400." });
  for (const m of ["AddMetricAsync", "AddMetricsAsync", "LogMetricAsync", "LogMetricsAsync",
                   "RetrieveAllMetricsAsync", "RetrieveMetricByIdAsync", "RemoveMetricByIdAsync",
                   "PurgeMetricsOlderThanRetentionPeriodAsync"])
    D(["LIB.AM.MetricClient", m], ["LIB.AM.MetricService", m]);

  C({ id: "LIB.AM.AuditService", name: "AuditService", project: "clients-am", layer: "foundation", col: 11,
      methods: ["LogAuditAsync", "RecordAuditAsync", "AddAuditAsync", "BulkLogAuditsAsync", "BulkAddAuditsAsync",
                "RetrieveAllAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync"],
      description: "Validates and stamps the entry, then persists through the storage port. The Log* verbs defer only the write — everything above it runs on the caller's thread, so an entry is stamped with the time the event happened rather than the time the queue drained. Record* and Add* are awaited, for entries that must not be lost." });
  for (const m of ["LogAuditAsync", "RecordAuditAsync", "AddAuditAsync"])
    D(["LIB.AM.AuditService", m], ["AuditAndMetricStorageBroker", "InsertAuditAsync"]);
  for (const m of ["BulkLogAuditsAsync", "BulkAddAuditsAsync"])
    D(["LIB.AM.AuditService", m], ["AuditAndMetricStorageBroker", "BulkInsertAuditsAsync"]);
  D(["LIB.AM.AuditService", "RetrieveAllAuditsAsync"], ["AuditAndMetricStorageBroker", "SelectAllAuditsAsync"]);
  D(["LIB.AM.AuditService", "RetrieveAuditByIdAsync"], ["AuditAndMetricStorageBroker", "SelectAuditByIdAsync"]);
  D(["LIB.AM.AuditService", "ModifyAuditAsync"], ["AuditAndMetricStorageBroker", "SelectAuditByIdAsync"]);
  D(["LIB.AM.AuditService", "ModifyAuditAsync"], ["AuditAndMetricStorageBroker", "UpdateAuditAsync"]);
  D(["LIB.AM.AuditService", "RemoveAuditByIdAsync"], ["AuditAndMetricStorageBroker", "SelectAuditByIdAsync"]);
  D(["LIB.AM.AuditService", "RemoveAuditByIdAsync"], ["AuditAndMetricStorageBroker", "DeleteAuditAsync"]);
  D(["LIB.AM.AuditService", "LogAuditAsync"], ["AuditAndMetricStorageBroker", "CreateAudit"]);
  D(["LIB.AM.AuditService", "LogAuditAsync"], ["AuditUserBroker", "GetCurrentUserIdAsync"]);
  D(["LIB.AM.AuditService", "LogAuditAsync"], ["API.Dispatcher", "TryDispatch"]);
  D(["LIB.AM.AuditService", "BulkLogAuditsAsync"], ["API.Dispatcher", "TryDispatch"]);

  C({ id: "LIB.AM.MetricService", name: "MetricService", project: "clients-am", layer: "foundation", col: 11,
      methods: ["AddMetricAsync", "AddMetricsAsync", "LogMetricAsync", "LogMetricsAsync",
                "RetrieveAllMetricsAsync", "RetrieveMetricByIdAsync", "RemoveMetricByIdAsync",
                "PurgeMetricsOlderThanRetentionPeriodAsync"],
      description: "The fan-out point: one span, two destinations. Every write persists through the storage port and then publishes the same span to the telemetry sink. Recording is governed by IsEnabled, and the retention sweep by IsPurgingAllowed / RetentionPeriodInDays / PurgeBatchSize. It holds both a metricStorageBroker and a metricBroker — the port it persists through and this library's own ActivitySource sink." });
  for (const m of ["AddMetricAsync", "LogMetricAsync"])
    D(["LIB.AM.MetricService", m], ["AuditAndMetricStorageBroker", "InsertMetricAsync"]);
  for (const m of ["AddMetricsAsync", "LogMetricsAsync"])
    D(["LIB.AM.MetricService", m], ["AuditAndMetricStorageBroker", "BulkInsertMetricsAsync"]);
  D(["LIB.AM.MetricService", "RetrieveAllMetricsAsync"], ["AuditAndMetricStorageBroker", "SelectAllMetricsAsync"]);
  D(["LIB.AM.MetricService", "RetrieveMetricByIdAsync"], ["AuditAndMetricStorageBroker", "SelectMetricByIdAsync"]);
  D(["LIB.AM.MetricService", "RemoveMetricByIdAsync"], ["AuditAndMetricStorageBroker", "SelectMetricByIdAsync"]);
  D(["LIB.AM.MetricService", "RemoveMetricByIdAsync"], ["AuditAndMetricStorageBroker", "DeleteMetricAsync"]);
  D(["LIB.AM.MetricService", "PurgeMetricsOlderThanRetentionPeriodAsync"],
    ["AuditAndMetricStorageBroker", "DeleteMetricsOlderThanAsync"]);
  for (const m of ["AddMetricAsync", "AddMetricsAsync", "LogMetricAsync", "LogMetricsAsync"])
    D(["LIB.AM.MetricService", m], ["LIB.AM.MetricBroker", "RecordAsync"]);
  D(["LIB.AM.MetricService", "LogMetricsAsync"], ["API.Dispatcher", "TryDispatch"]);

  C({ id: "LIB.AM.MetricBroker", name: "MetricBroker (telemetry)", project: "clients-am", layer: "broker", col: 11,
      methods: ["RecordAsync"],
      description: "Emits completed spans to the telemetry pipeline — the second sink for the same spans the storage port persists. The ActivitySource is cached by name and shared across instances, because a source registers itself with the diagnostics subsystem for its lifetime and this broker is transient inside a scoped client. Recording is best effort and never affects the measured request. Named IMetricBroker in its own namespace; not to be confused with Core.Abstractions' IMetricStorageBroker-era naming." });
  D(["LIB.AM.MetricBroker", "RecordAsync"], ["EXT.ActivitySource", "StartActivity / SetTag"]);

  /* ==================================================================
     LondonFhirService.Core.Abstractions — the ports the library
     declares and the hosting application implements.
     ================================================================== */
  C({ id: "ABS.Ports", name: "Audit & metric ports", project: "core-abstractions", layer: "broker", col: 9, shared: true,
      methods: ["IAuditAndMetricStorageBroker", "IAuditUserBroker", "IAuditAndMetricsDispatcher"],
      description: "Declared here rather than consumed from the hosting application, so the dependency runs one way — application to library — and the reference stays acyclic while the library still writes to the application's database. Everything is expressed in IAudit / IMetric; the library never sees the concrete entities or the ORM. Implementations also classify storage failures: the library carries no ORM so it cannot name SqlException or DbUpdateException, and needs those arriving as the exceptions in Models.Audits/Metrics.Exceptions." });
  D(["ABS.Ports", "IAuditAndMetricStorageBroker"], ["AuditAndMetricStorageBroker", null]);
  D(["ABS.Ports", "IAuditUserBroker"], ["AuditUserBroker", null]);
  D(["ABS.Ports", "IAuditAndMetricsDispatcher"], ["API.Dispatcher", null]);

  /* ==================================================================
     LondonFhirService.Core — brokers.
     ================================================================== */
  C({ id: "StorageBroker", name: "StorageBroker", project: "core", layer: "broker", col: 8,
      methods: [],
      description: "Partial EF Core DbContext (derives from EFxceptions' EFxceptionsContext) with one partial per entity. Most methods delegate to a private generic that calls STX.EFCore.Client's EFCoreClient. It also inherits IAuditAndMetricStorageBroker, so the audit and metric members are declared once in Core.Abstractions over IAudit / IMetric — which is what lets the standalone library share this broker without Core handing it a concrete type. Registered both as a DbContextFactory and a scoped IStorageBroker; OnConfiguring reads the LondonFhirServiceConnectionString and enables HierarchyId." });

  C({ id: "StorageBrokerFactory", name: "StorageBrokerFactory", project: "core", layer: "broker", col: 8,
      methods: ["CreateStorageBrokerAsync"],
      description: "Wraps IDbContextFactory<StorageBroker>. AuditAndMetricStorageBroker and Stu3PatientService use it to get a short-lived, independently-disposed StorageBroker instead of the scoped one — they write from background and parallel work where the request-scoped context is unsafe." });
  D(["StorageBrokerFactory", "CreateStorageBrokerAsync"], ["EXT.EFCore", "IDbContextFactory.CreateDbContextAsync"]);

  C({ id: "SecurityAuditBroker", name: "SecurityAuditBroker", project: "core", layer: "broker", col: 8,
      methods: ["ApplyAddAuditValuesAsync", "ApplyModifyAuditValuesAsync", "ApplyRemoveAuditValuesAsync", "EnsureAddAuditValuesRemainsUnchangedOnModifyAsync", "GetUserIdAsync"],
      description: "Stamps CreatedBy/CreatedDate/UpdatedBy/UpdatedDate (and DeletedBy/DeletedDate) using the SecurityConfigurations built in each host's AddBrokers. Delegates to ISL.Security.Client's Audits client." });
  D(["SecurityAuditBroker", "ApplyAddAuditValuesAsync"], ["LIB.SecurityClient", "Audits.ApplyAddAuditValuesAsync"]);
  D(["SecurityAuditBroker", "ApplyModifyAuditValuesAsync"], ["LIB.SecurityClient", "Audits.ApplyModifyAuditValuesAsync"]);
  D(["SecurityAuditBroker", "ApplyRemoveAuditValuesAsync"], ["LIB.SecurityClient", "Audits.ApplyRemoveAuditValuesAsync"]);
  D(["SecurityAuditBroker", "EnsureAddAuditValuesRemainsUnchangedOnModifyAsync"], ["LIB.SecurityClient", "Audits.EnsureOtherAuditValuesRemainsUnchangedOnModifyAsync"]);
  D(["SecurityAuditBroker", "GetUserIdAsync"], ["LIB.SecurityClient", "Audits.GetUserIdAsync"]);

  C({ id: "SecurityBroker", name: "SecurityBroker", project: "core", layer: "broker", col: 8,
      methods: ["GetCurrentUserAsync", "IsCurrentUserAuthenticatedAsync", "IsInRoleAsync", "HasClaimAsync", "ValidateCaptchaAsync", "GetIpAddressAsync"],
      description: "Reads the caller from the ClaimsPrincipal. Only GetCurrentUserAsync is consumed today (by Stu3PatientOrchestrationService, to build the ValidateAccessRequest). ValidateCaptchaAsync is stubbed to return true — the captcha providers are registered but not yet called." });
  D(["SecurityBroker", "GetCurrentUserAsync"], ["LIB.SecurityClient", "Users.GetUserAsync"]);
  D(["SecurityBroker", "IsCurrentUserAuthenticatedAsync"], ["LIB.SecurityClient", "Users.IsUserAuthenticatedAsync"]);
  D(["SecurityBroker", "IsInRoleAsync"], ["LIB.SecurityClient", "Users.IsUserInRoleAsync"]);
  D(["SecurityBroker", "HasClaimAsync"], ["LIB.SecurityClient", "Users.UserHasClaimAsync"]);

  /* -- the utility broker every layer records through -- */
  C({ id: "AuditAndMetricBroker", name: "AuditAndMetricBroker", project: "core", layer: "broker", col: 8,
      methods: ["LogInformationAsync", "RecordAuditAsync", "LogAuditAsync", "BulkLogAuditsAsync", "AddAuditAsync",
                "RetrieveAllAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync",
                "LogMetricAsync", "LogMetricsAsync", "AddMetricAsync",
                "RetrieveAllMetricsAsync", "RetrieveMetricByIdAsync", "RemoveMetricByIdAsync",
                "PurgeMetricsOlderThanRetentionPeriodAsync"],
      description: "The utility broker any service can call to record an audit entry or a metric span. Recording is service work, but every layer needs it and a broker may not call a service — so those services live in LondonFhirService.Clients.AuditAndMetrics and this broker wraps an external dependency, which is what a broker is for. It stamps nothing and constructs nothing: calling services build their own entries so the timestamp is the event time rather than the submit time. Not to be confused with AuditAndMetricStorageBroker beside it — this one runs up into the library, that one runs down into storage." });
  for (const m of ["LogInformationAsync", "RecordAuditAsync", "LogAuditAsync", "AddAuditAsync",
                   "RetrieveAllAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync"])
    D(["AuditAndMetricBroker", m], ["LIB.AMClient", "AuditClient"]);
  D(["AuditAndMetricBroker", "BulkLogAuditsAsync"], ["LIB.AMClient", "AuditClient"]);
  for (const m of ["LogMetricAsync", "LogMetricsAsync", "AddMetricAsync", "RetrieveAllMetricsAsync",
                   "RetrieveMetricByIdAsync", "RemoveMetricByIdAsync", "PurgeMetricsOlderThanRetentionPeriodAsync"])
    D(["AuditAndMetricBroker", m], ["LIB.AMClient", "MetricClient"]);

  /* -- the two port adapters the library calls back down into -- */
  C({ id: "AuditAndMetricStorageBroker", name: "AuditAndMetricStorageBroker", project: "core", layer: "broker", col: 8,
      methods: ["CreateAudit", "InsertAuditAsync", "BulkInsertAuditsAsync", "SelectAllAuditsAsync", "SelectAuditByIdAsync",
                "UpdateAuditAsync", "DeleteAuditAsync",
                "InsertMetricAsync", "BulkInsertMetricsAsync", "SelectAllMetricsAsync", "SelectMetricByIdAsync",
                "DeleteMetricAsync", "DeleteMetricsOlderThanAsync"],
      description: "Satisfies the storage port the audit and metrics library declares, the same way AuditUserBroker satisfies its identity port. Classifying storage failures is part of the port's contract rather than logic of its own — the library carries no ORM, so it cannot name SqlException. Writes go through the factory and get their own short-lived context, which is what makes them safe to fire and forget: a write dispatched to the background outlives the request scope. Reads keep the scoped broker, because they hand back an IQueryable the caller enumerates. The arrows into this component run right-to-left: the library calls back down into the application that hosts it." });
  for (const m of ["InsertAuditAsync", "BulkInsertAuditsAsync", "UpdateAuditAsync", "DeleteAuditAsync",
                   "InsertMetricAsync", "BulkInsertMetricsAsync", "DeleteMetricAsync", "DeleteMetricsOlderThanAsync"])
    D(["AuditAndMetricStorageBroker", m], ["StorageBrokerFactory", "CreateStorageBrokerAsync"]);
  D(["AuditAndMetricStorageBroker", "InsertAuditAsync"], ["StorageBroker", "InsertAuditAsync"]);
  D(["AuditAndMetricStorageBroker", "BulkInsertAuditsAsync"], ["StorageBroker", "BulkInsertAuditsAsync"]);
  D(["AuditAndMetricStorageBroker", "SelectAllAuditsAsync"], ["StorageBroker", "SelectAllAuditsAsync"]);
  D(["AuditAndMetricStorageBroker", "SelectAuditByIdAsync"], ["StorageBroker", "SelectAuditByIdAsync"]);
  D(["AuditAndMetricStorageBroker", "UpdateAuditAsync"], ["StorageBroker", "UpdateAuditAsync"]);
  D(["AuditAndMetricStorageBroker", "DeleteAuditAsync"], ["StorageBroker", "DeleteAuditAsync"]);
  D(["AuditAndMetricStorageBroker", "InsertMetricAsync"], ["StorageBroker", "InsertMetricAsync"]);
  D(["AuditAndMetricStorageBroker", "BulkInsertMetricsAsync"], ["StorageBroker", "BulkInsertMetricsAsync"]);
  D(["AuditAndMetricStorageBroker", "SelectAllMetricsAsync"], ["StorageBroker", "SelectAllMetricsAsync"]);
  D(["AuditAndMetricStorageBroker", "SelectMetricByIdAsync"], ["StorageBroker", "SelectMetricByIdAsync"]);
  D(["AuditAndMetricStorageBroker", "DeleteMetricAsync"], ["StorageBroker", "DeleteMetricAsync"]);
  D(["AuditAndMetricStorageBroker", "DeleteMetricsOlderThanAsync"], ["StorageBroker", "DeleteMetricsOlderThanAsync"]);

  C({ id: "AuditUserBroker", name: "AuditUserBroker", project: "core", layer: "broker", col: 8,
      methods: ["GetCurrentUserIdAsync"],
      description: "Satisfies the audit library's identity port from this application's security broker. SecurityAuditBroker captures the ClaimsPrincipal in its constructor rather than reading it per call, so this must be resolved per request — registered as a singleton it would be built at startup with no HttpContext, and every audit row in the system would be stamped anonymous with nothing failing to signal it." });
  D(["AuditUserBroker", "GetCurrentUserIdAsync"], ["SecurityAuditBroker", "GetUserIdAsync"]);

  C({ id: "ConsumerAccessBroker", name: "ConsumerAccessBroker", project: "core", layer: "broker", col: 8,
      methods: ["CheckConsumerAccessAsync"],
      description: "Posts a ValidateAccessRequest to a remote consumer-access endpoint with a DefaultAzureCredential bearer token and reads back a ConsumerAccess verdict. Registered in the API host (AddHttpClient) and consumed by ConsumerAccessService — this is now the whole access decision." });
  D(["ConsumerAccessBroker", "CheckConsumerAccessAsync"], ["EXT.TokenCredential", "GetTokenAsync"]);
  D(["ConsumerAccessBroker", "CheckConsumerAccessAsync"], ["EXT.HttpClient", "SendAsync"]);

  /* -- utility brokers (hidden behind the toggle) -- */
  C({ id: "DateTimeBroker", name: "DateTimeBroker", project: "core", layer: "broker", col: 8, utility: true,
      methods: ["GetCurrentDateTimeOffsetAsync"] });
  C({ id: "IdentifierBroker", name: "IdentifierBroker", project: "core", layer: "broker", col: 8, utility: true,
      methods: ["GetIdentifierAsync"] });
  C({ id: "LoggingBroker", name: "LoggingBroker", project: "core", layer: "broker", col: 8, utility: true,
      methods: ["LogInformationAsync", "LogTraceAsync", "LogDebugAsync", "LogWarningAsync", "LogErrorAsync", "LogCriticalAsync"],
      description: "ILogger passthrough. Exception-path (TryCatch) logging is deliberately NOT drawn on this graph — only happy-path calls and denial logging." });
  for (const [from, to] of [
    ["LogInformationAsync", "ILogger.LogInformation"], ["LogTraceAsync", "ILogger.LogTrace"],
    ["LogDebugAsync", "ILogger.LogDebug"], ["LogWarningAsync", "ILogger.LogWarning"],
    ["LogErrorAsync", "ILogger.LogError"], ["LogCriticalAsync", "ILogger.LogCritical"],
  ]) D(["LoggingBroker", from], ["EXT.ILogger", to]);

  /* -- Stu3FhirBroker: forwards the full typed-resource surface -- */
  const FHIR_RESOURCES = [
    "Accounts", "ActivityDefinitions", "AllergyIntolerances", "AppointmentResponses", "Appointments",
    "AuditEvents", "Basics", "Binaries", "Bundles", "CapabilityStatements", "CarePlans", "CareTeams",
    "ClaimResponses", "Claims", "ClinicalImpressions", "CodeSystems", "CommunicationRequests",
    "Communications", "CompartmentDefinitions", "Compositions", "ConceptMaps", "Conditions", "Consents",
    "Contracts", "Coverages", "DetectedIssues", "DeviceMetrics", "DeviceRequests", "Devices",
    "DeviceUseStatements", "DiagnosticReports", "DocumentManifests", "DocumentReferences", "Encounters",
    "Endpoints", "EnrollmentRequests", "EnrollmentResponses", "EpisodeOfCare", "ExplanationsOfBenefits",
    "FamilyMemberHistories", "Flags", "Goals", "GraphDefinitions", "Groups", "GuidanceResponses",
    "HealthcareServices", "ImagingStudies", "ImmunizationRecommendations", "Immunizations",
    "ImplementationGuides", "Libraries", "Linkages", "Lists", "Locations", "MeasureReports", "Measures",
    "Media", "MedicationAdministrations", "MedicationDispenses", "MedicationRequests", "Medications",
    "MedicationStatements", "MessageDefinitions", "MessageHeaders", "NamingSystems", "NutritionOrders",
    "Observations", "OperationDefinitions", "OperationOutcomes", "Organizations", "Parameters", "Patients",
    "PaymentNotices", "PaymentReconciliations", "Persons", "PlanDefinitions", "PractitionerRoles",
    "Practitioners", "Procedures", "Provenances", "QuestionnaireResponses", "Questionnaires",
    "RelatedPersons", "RequestGroups", "ResearchStudies", "ResearchSubjects", "RiskAssessments",
    "Schedules", "SearchParameters", "Slots", "Specimens", "StructureDefinitions", "StructureMaps",
    "Subscriptions", "Substances", "SupplyDeliveries", "SupplyRequests", "Tasks", "TestReports",
    "TestScripts", "ValueSets", "VisionPrescriptions",
  ];
  // three accessors are named differently on the abstraction provider
  const FHIR_RENAMES = {
    EpisodeOfCare: "EpisodeOfCares",
    ExplanationsOfBenefits: "ExplanationOfBenefits",
    Parameters: "Parameterss",
  };

  C({ id: "Stu3FhirBroker", name: "Stu3FhirBroker", project: "core", layer: "broker", col: 8,
      methods: ["FhirProviders"].concat(FHIR_RESOURCES),
      description: `Thin passthrough over IFhirAbstractionProvider. It exposes ${FHIR_RESOURCES.length} typed STU3 resource accessors, but the solution consumes exactly one member today — the FhirProviders collection, which Stu3PatientService filters by provider name. The rest of the surface is registered and forwarded but never called.` });
  D(["Stu3FhirBroker", "FhirProviders"], ["LIB.FhirAbstractionProvider", "FhirProviders"]);
  for (const r of FHIR_RESOURCES) D(["Stu3FhirBroker", r], ["LIB.FhirAbstractionProvider", FHIR_RENAMES[r] || r]);

  /* ==================================================================
     LondonFhirService.Core — foundation services.

     Three entities share one CRUD template; three read/write variants:
       A audited CRUD — security-audit stamps on add/modify, hard delete
       B audited CRUD — as A, plus a remove-audit stamp + Update before
         the Delete (soft-delete values are persisted first)
       C plain CRUD  — no security-audit broker at all
     ================================================================== */
  const CRUD_ENTITIES = [
    { e: "FhirRecord", plural: "FhirRecords", variant: "A", extras: ["TryClaimFhirRecordAsync"] },
    { e: "FhirRecordDifference", plural: "FhirRecordDifferences", variant: "A" },
    { e: "Provider", plural: "Providers", variant: "A", extras: ["RetrieveAllProvidersAsListAsync"] },
  ];

  /* Every entity's StorageBroker surface. Audits and metrics are not in
     CRUD_ENTITIES — their rows come from the port Core.Abstractions
     declares, and metrics have no Update because a span is append-only. */
  const STORAGE_ENTITIES = CRUD_ENTITIES.map(c => ({ ...c, update: true, del: true })).concat([
    { e: "Audit", plural: "Audits", bulk: true, update: true, del: true },
    { e: "Metric", plural: "Metrics", bulk: true, update: false, del: true,
      extras: ["DeleteMetricsOlderThanAsync"] },
  ]);
  const storageBroker = components.find(c => c.id === "StorageBroker");
  for (const cfg of STORAGE_ENTITIES) {
    const rows = [];
    if (cfg.bulk) rows.push(`BulkInsert${cfg.plural}Async`);
    rows.push(`Insert${cfg.e}Async`, `SelectAll${cfg.plural}Async`, `Select${cfg.e}ByIdAsync`);
    if (cfg.update) rows.push(`Update${cfg.e}Async`);
    if (cfg.del) rows.push(`Delete${cfg.e}Async`);
    storageBroker.methods.push(...rows);

    if (cfg.bulk) D(["StorageBroker", `BulkInsert${cfg.plural}Async`], ["LIB.EFCoreClient", "BulkInsertAsync"]);
    D(["StorageBroker", `Insert${cfg.e}Async`], ["LIB.EFCoreClient", "InsertAsync"]);
    D(["StorageBroker", `SelectAll${cfg.plural}Async`], ["LIB.EFCoreClient", "SelectAllAsync"]);
    D(["StorageBroker", `Select${cfg.e}ByIdAsync`], ["LIB.EFCoreClient", "SelectAsync"]);
    if (cfg.update) D(["StorageBroker", `Update${cfg.e}Async`], ["LIB.EFCoreClient", "UpdateAsync"]);
    if (cfg.del) D(["StorageBroker", `Delete${cfg.e}Async`], ["LIB.EFCoreClient", "DeleteAsync"]);
  }

  /* Three storage methods bypass EFCoreClient and hit EF Core directly:
     two set-based statements and one straight materialisation. */
  storageBroker.methods.push("ClaimFhirRecordAsync", "SelectAllProvidersAsListAsync", "DeleteMetricsOlderThanAsync");
  D(["StorageBroker", "ClaimFhirRecordAsync"], ["EXT.EFCore", "ExecuteUpdateAsync"]);
  D(["StorageBroker", "DeleteMetricsOlderThanAsync"], ["EXT.EFCore", "ExecuteDeleteAsync"]);
  D(["StorageBroker", "SelectAllProvidersAsListAsync"], ["EXT.EFCore", "ToListAsync"]);

  for (const m of ["InsertAsync", "SelectAllAsync", "SelectAsync", "UpdateAsync", "DeleteAsync",
                   "BulkInsertAsync", "BulkUpdateAsync", "BulkDeleteAsync"])
    D(["LIB.EFCoreClient", m], ["EXT.EFCore", "DbContext (this StorageBroker)"]);

  for (const cfg of CRUD_ENTITIES) {
    const e = cfg.e, v = cfg.variant, plural = cfg.plural, svc = "FS." + e;
    const add = `Add${e}Async`, retAll = `RetrieveAll${plural}Async`, retById = `Retrieve${e}ByIdAsync`;
    const modify = `Modify${e}Async`, remove = `Remove${e}ByIdAsync`;
    const methods = [add, retAll, retById, modify, remove].concat(cfg.extras || []);

    const variantNote = {
      A: "Add and Modify take security-audit stamps; Modify also re-reads storage and asks the security-audit broker to keep the original Created values; Remove is a straight delete after a read.",
      B: "As the audited template, plus Remove stamps remove-audit values and persists them with an Update before the Delete.",
      C: "No security-audit broker — audit columns arrive already populated on the model.",
    }[v];

    C({ id: svc, name: e + "Service", project: "core", layer: "foundation", col: 7, methods,
        description: `Foundation CRUD for ${e}. ${variantNote} Validations run inside the same TryCatch as the happy path, so their broker calls are drawn here too.` });

    const st = (from, to) => D([svc, from], ["StorageBroker", to]);
    const sa = (from, to) => D([svc, from], ["SecurityAuditBroker", to]);
    const dt = (from) => D([svc, from], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);

    // -- Add
    if (v !== "C") { sa(add, "ApplyAddAuditValuesAsync"); sa(add, "GetUserIdAsync"); dt(add); }
    st(add, `Insert${e}Async`);

    // -- RetrieveAll / RetrieveById
    st(retAll, `SelectAll${plural}Async`);
    st(retById, `Select${e}ByIdAsync`);

    // -- Modify
    if (v !== "C") { sa(modify, "ApplyModifyAuditValuesAsync"); sa(modify, "GetUserIdAsync"); dt(modify); }
    st(modify, `Select${e}ByIdAsync`);
    if (v === "A") sa(modify, "EnsureAddAuditValuesRemainsUnchangedOnModifyAsync");
    st(modify, `Update${e}Async`);

    // -- Remove
    st(remove, `Select${e}ByIdAsync`);
    if (v === "B") { sa(remove, "ApplyRemoveAuditValuesAsync"); dt(remove); st(remove, `Update${e}Async`); }
    st(remove, `Delete${e}Async`);
  }

  /* -- the two structural deviations from the CRUD template -- */
  D(["FS.FhirRecord", "TryClaimFhirRecordAsync"], ["StorageBroker", "ClaimFhirRecordAsync"]);
  D(["FS.FhirRecord", "TryClaimFhirRecordAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["FS.Provider", "RetrieveAllProvidersAsListAsync"], ["StorageBroker", "SelectAllProvidersAsListAsync"]);

  /* -- ConsumerAccessService: a pure passthrough over the remote
        consumer-access API, so it owns no storage surface at all -- */
  C({ id: "FS.ConsumerAccess", name: "ConsumerAccessService", project: "core", layer: "foundation", col: 7,
      methods: ["CheckConsumerAccessAsync"],
      description: "A single passthrough onto ConsumerAccessBroker, consumed by Stu3PatientOrchestrationService's access check. Validates the ValidateAccessRequest (ConsumerUserId, NhsNumber, CorrelationId) and forwards it with the caller's CancellationToken; its TryCatch maps HttpRequestException to a critical dependency exception and timeout / cancellation onto TimedOut- and CancelledConsumerAccessServiceException. It takes no storage broker — consumer access is no longer held locally." });
  D(["FS.ConsumerAccess", "CheckConsumerAccessAsync"], ["ConsumerAccessBroker", "CheckConsumerAccessAsync"]);

  /* -- AuditService / MetricService: the management surface over the
        audit and metrics library. Neither touches storage: both go
        through the one AuditAndMetricBroker. -- */
  const AUDIT_ADD_ARGS = "AddAuditAsync(auditType, title, message, …)";
  C({ id: "FS.Audit", name: "AuditService", project: "core", layer: "foundation", col: 7,
      methods: [AUDIT_ADD_ARGS, "AddAuditAsync(audit)", "BulkAddAuditsAsync", "RetrieveAllAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync"],
      description: "Delegates to the audit and metrics broker rather than reaching for storage. Validation and stamping now live in the library behind that broker, so what remains is the API surface this application exposes and the localisation of the client's exceptions into this service's own — controllers dispatch on Core's categorization types to choose a status code. There is deliberately no Validations partial: duplicating the library's rules here would let the two drift. AddAuditAsync(audit) and ModifyAuditAsync stamp through the security-audit broker, overwriting whatever the caller sent, because an entity arriving from a request body carries claims rather than facts." });
  D(["FS.Audit", AUDIT_ADD_ARGS], ["AuditAndMetricBroker", "AddAuditAsync"]);
  D(["FS.Audit", "AddAuditAsync(audit)"], ["SecurityAuditBroker", "ApplyAddAuditValuesAsync"]);
  D(["FS.Audit", "AddAuditAsync(audit)"], ["AuditAndMetricBroker", "AddAuditAsync"]);
  D(["FS.Audit", "BulkAddAuditsAsync"], ["AuditAndMetricBroker", "BulkLogAuditsAsync"]);
  D(["FS.Audit", "RetrieveAllAuditsAsync"], ["AuditAndMetricBroker", "RetrieveAllAuditsAsync"]);
  D(["FS.Audit", "RetrieveAuditByIdAsync"], ["AuditAndMetricBroker", "RetrieveAuditByIdAsync"]);
  D(["FS.Audit", "ModifyAuditAsync"], ["SecurityAuditBroker", "ApplyModifyAuditValuesAsync"]);
  D(["FS.Audit", "ModifyAuditAsync"], ["AuditAndMetricBroker", "ModifyAuditAsync"]);
  D(["FS.Audit", "RemoveAuditByIdAsync"], ["AuditAndMetricBroker", "RemoveAuditByIdAsync"]);

  C({ id: "FS.Metric", name: "MetricService", project: "core", layer: "foundation", col: 7,
      methods: ["AddMetricAsync", "LogMetricAsync", "LogMetricsAsync", "RetrieveAllMetricsAsync",
                "RetrieveMetricByIdAsync", "RemoveMetricByIdAsync", "PurgeMetricsOlderThanRetentionPeriodAsync"],
      description: "The metric counterpart to AuditService, and the newer of the two. Callers used to reach AuditAndMetricBroker directly for metrics, which left metric failures arriving at controllers and workers in the library's exception types while audit failures arrived in this application's — this service closes that gap. Add is the awaited API surface; Log is fire and forget. Nothing is stamped here: a span carries its own Started and Completed, taken when the work happened. There is no Modify — a metric records what already happened, so the table is append-only." });
  D(["FS.Metric", "AddMetricAsync"], ["AuditAndMetricBroker", "AddMetricAsync"]);
  D(["FS.Metric", "LogMetricAsync"], ["AuditAndMetricBroker", "LogMetricAsync"]);
  D(["FS.Metric", "LogMetricsAsync"], ["AuditAndMetricBroker", "LogMetricsAsync"]);
  D(["FS.Metric", "RetrieveAllMetricsAsync"], ["AuditAndMetricBroker", "RetrieveAllMetricsAsync"]);
  D(["FS.Metric", "RetrieveMetricByIdAsync"], ["AuditAndMetricBroker", "RetrieveMetricByIdAsync"]);
  D(["FS.Metric", "RemoveMetricByIdAsync"], ["AuditAndMetricBroker", "RemoveMetricByIdAsync"]);
  D(["FS.Metric", "PurgeMetricsOlderThanRetentionPeriodAsync"],
    ["AuditAndMetricBroker", "PurgeMetricsOlderThanRetentionPeriodAsync"]);

  /* -- STU3 patient retrieval -- */
  C({ id: "FS.Stu3Patient", name: "Stu3PatientService", project: "core", layer: "foundation", col: 7,
      methods: ["GetStructuredRecordSerialisedAsync", "QueueFhirRecordPersistenceAsync", "RecordFailedProviderSpanAsync"],
      description: "Fans out to every active provider in parallel (Task.WhenAll), each call wrapped in a linked CancellationTokenSource that cancels after PatientServiceConfig.MaxProviderWaitTimeMilliseconds. Each returned bundle is queued for persistence as a Pending FhirRecord through the dispatcher, so the write leaves the request's critical path, and each provider call emits a metric span — including a failure span when a provider times out or throws. Providers that do not support Patients/$GetStructuredRecord are dropped before the fan-out." });
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditAndMetricBroker", "LogInformationAsync"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["Stu3FhirBroker", "FhirProviders"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "ProviderName"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "SupportsResource"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "DisplayName"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "Patients.GetStructuredRecordSerialisedAsync"]);
  D(["FS.Stu3Patient", "QueueFhirRecordPersistenceAsync"], ["API.Dispatcher", "TryDispatch"]);
  D(["FS.Stu3Patient", "QueueFhirRecordPersistenceAsync"], ["AuditAndMetricBroker", "LogMetricAsync"]);
  D(["FS.Stu3Patient", "QueueFhirRecordPersistenceAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["FS.Stu3Patient", "QueueFhirRecordPersistenceAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["FS.Stu3Patient", "QueueFhirRecordPersistenceAsync"], ["SecurityAuditBroker", "ApplyAddAuditValuesAsync"]);
  D(["FS.Stu3Patient", "QueueFhirRecordPersistenceAsync"], ["StorageBrokerFactory", "CreateStorageBrokerAsync"]);
  D(["FS.Stu3Patient", "QueueFhirRecordPersistenceAsync"], ["StorageBroker", "InsertFhirRecordAsync"]);
  D(["FS.Stu3Patient", "RecordFailedProviderSpanAsync"], ["AuditAndMetricBroker", "LogMetricAsync"]);

  C({ id: "FS.JsonElement", name: "JsonElementService", project: "core", layer: "foundation", col: 7,
      methods: ["CreateStringElement", "CreateArrayElement", "CreateObjectElement"],
      description: "Pure System.Text.Json factory (registered as a singleton). No brokers — it writes through Utf8JsonWriter into a MemoryStream and clones the parsed root element." });

  /* -- resource matchers: 21 services on one template -- */
  const MATCHERS = [
    ["AllergyIntolerance", "AllergyIntolerances"], ["Appointment", "Appointments"],
    ["Condition", "Conditions"], ["DiagnosticReport", "DiagnosticReports"],
    ["Encounter", "Encounters"], ["EpisodeOfCare", "EpisodeOfCares"],
    ["FamilyMemberHistory", "FamilyMemberHistories"], ["Immunization", "Immunizations"],
    ["List", "Lists"], ["Location", "Locations"], ["Medication", "Medications"],
    ["MedicationRequest", "MedicationRequests"], ["MedicationStatement", "MedicationStatements"],
    ["Observation", "Observations"], ["Organization", "Organizations"], ["Patient", "Patients"],
    ["Practitioner", "Practitioners"], ["PractitionerRole", "PractitionerRoles"],
    ["Procedure", "Procedures"], ["ProcedureRequest", "ProcedureRequests"],
    ["ReferralRequest", "ReferralRequests"],
  ];
  for (const [resource] of MATCHERS) {
    C({ id: "FS.Matcher." + resource, name: resource + "MatcherService", project: "core", layer: "foundation", col: 7,
        methods: ["ResourceType", "GetMatchKeyAsync", "MatchAsync"], shared: true,
        description: `Derives a stable match key for ${resource} resources and pairs Source1 against Source2 into Matched / Unmatched sets. Derives from ResourceMatcherServiceBase; ILoggingBroker is its only dependency and it is used on the exception path only, so no edges are drawn. Marked "shared" — the registry is a DI fan-in, so every consumer links to one copy instead of getting 21 duplicates per chain.` });
  }

  /* ==================================================================
     LondonFhirService.Core — processing services.
     ================================================================== */
  C({ id: "PR.ResourceMatcher", name: "ResourceMatcherProcessingService", project: "core", layer: "processing", col: 6,
      methods: ["GetMatcherAsync", "HasMatcherAsync"],
      description: `Indexes every DI-registered IResourceMatcherService by its ResourceType (case-insensitive) and hands the right one back. All ${MATCHERS.length} matchers are injected as IEnumerable<IResourceMatcherService>, so the links below are constructor fan-in rather than per-call dispatch.` });
  for (const [resource] of MATCHERS) D(["PR.ResourceMatcher", "GetMatcherAsync"], ["FS.Matcher." + resource, null]);

  C({ id: "PR.ListEntryComparison", name: "ListEntryComparisonProcessingService", project: "core", layer: "processing", col: 6,
      methods: ["CompareListEntryCountsAsync"],
      description: "Compares the entry count of two List resources and emits an entry-count-mismatch DiffItem when they differ. ILoggingBroker only, on the exception path." });

  const IGNORE_RULES = [
    ["ArrayOrder", "Sorts array members before comparison so ordering differences are not reported as diffs."],
    ["Guid", "Replaces GUID-shaped values with a constant so provider-minted identifiers do not diff."],
    ["Id", "Replaces resource `id` values with a constant."],
    ["Meta", "Replaces `meta` blocks (versionId / lastUpdated) with a constant."],
  ];
  for (const [name, note] of IGNORE_RULES) {
    C({ id: "PR.Rule." + name, name: name + "IgnoreProcessingRule", project: "core", layer: "processing", col: 6,
        methods: ["ShouldIgnoreAsync", "GetReplacementAsync"],
        description: `${note} Derives from JsonIgnoreProcessingRuleBase.` });
  }
  D(["PR.Rule.ArrayOrder", "GetReplacementAsync"], ["FS.JsonElement", "CreateArrayElement"]);
  D(["PR.Rule.ArrayOrder", "GetReplacementAsync"], ["FS.JsonElement", "CreateObjectElement"]);
  D(["PR.Rule.Guid", "GetReplacementAsync"], ["FS.JsonElement", "CreateStringElement"]);
  D(["PR.Rule.Id", "GetReplacementAsync"], ["FS.JsonElement", "CreateStringElement"]);
  D(["PR.Rule.Meta", "GetReplacementAsync"], ["FS.JsonElement", "CreateStringElement"]);

  /* ==================================================================
     LondonFhirService.Core — orchestrations.
     ================================================================== */
  C({ id: "OR.Stu3FhirReconciliation", name: "Stu3FhirReconciliationService", project: "core", layer: "orchestration", col: 5,
      methods: ["ReconcileSerialisedAsync"],
      description: "Placeholder reconciliation: returns the first non-empty bundle and throws NotFoundFhirReconciliationOrchestrationException when every provider came back empty. It takes only ILoggingBroker — no real merge across providers yet. Modelled as an orchestration: it sits alongside Stu3PatientOrchestrationService under the coordination service, and its exceptions are the FhirReconciliationOrchestration* family." });

  C({ id: "OR.Stu3Patient", name: "Stu3PatientOrchestrationService", project: "core", layer: "orchestration", col: 5,
      methods: ["GetStructuredRecordSerialisedAsync", "CheckAccessPermissionsAsync"],
      description: "Gates the request on access, then fans it out. GetStructuredRecordSerialisedAsync runs the access check first, reads the STU3 providers, keeps the active ones (IsActive plus the ActiveFrom/ActiveTo window) with the primary first, validates that exactly one primary exists, and returns a StructuredRecordsResponse for the coordination service to reconcile. CheckAccessPermissionsAsync honours AccessConfigurations.CheckAccessPermissions: off, it audits the skip and returns; on, it resolves the caller, builds a ValidateAccessRequest and asks ConsumerAccessService — not allowed writes an Access Forbidden audit carrying the returned reason codes and throws ForbiddenPatientOrchestrationException, allowed writes an Access Allowed audit naming the organisations that granted it. Both paths emit a metric span." });
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditAndMetricBroker", "LogInformationAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditAndMetricBroker", "LogMetricAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["FS.Provider", "RetrieveAllProvidersAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"]);
  D(["OR.Stu3Patient", "CheckAccessPermissionsAsync"], ["AuditAndMetricBroker", "LogInformationAsync"]);
  D(["OR.Stu3Patient", "CheckAccessPermissionsAsync"], ["AuditAndMetricBroker", "RecordAuditAsync"]);
  D(["OR.Stu3Patient", "CheckAccessPermissionsAsync"], ["AuditAndMetricBroker", "LogMetricAsync"]);
  D(["OR.Stu3Patient", "CheckAccessPermissionsAsync"], ["SecurityBroker", "GetCurrentUserAsync"]);
  D(["OR.Stu3Patient", "CheckAccessPermissionsAsync"], ["FS.ConsumerAccess", "CheckConsumerAccessAsync"]);
  D(["OR.Stu3Patient", "CheckAccessPermissionsAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["OR.Stu3Patient", "CheckAccessPermissionsAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);

  C({ id: "OR.CompareQueue", name: "CompareQueueOrchestrationService", project: "core", layer: "orchestration", col: 5,
      methods: ["GetUnprocessedRecordAsync", "ChangeFhirRecordStatusAsync", "PersistFhirRecordDifferencesAsync"],
      description: "The comparison queue. GetUnprocessedRecordAsync claims the oldest Pending secondary FhirRecord that has been settled for at least 5 minutes and pairs it with the primary record sharing its CorrelationId. The claim is a set-based statement in the database rather than a read-then-write, so two workers cannot both win the same row." });
  D(["OR.CompareQueue", "GetUnprocessedRecordAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["OR.CompareQueue", "GetUnprocessedRecordAsync"], ["FS.FhirRecord", "RetrieveAllFhirRecordsAsync"]);
  D(["OR.CompareQueue", "GetUnprocessedRecordAsync"], ["FS.FhirRecord", "TryClaimFhirRecordAsync"]);
  D(["OR.CompareQueue", "ChangeFhirRecordStatusAsync"], ["FS.FhirRecord", "RetrieveFhirRecordByIdAsync"]);
  D(["OR.CompareQueue", "ChangeFhirRecordStatusAsync"], ["FS.FhirRecord", "ModifyFhirRecordAsync"]);
  D(["OR.CompareQueue", "PersistFhirRecordDifferencesAsync"], ["FS.FhirRecordDifference", "AddFhirRecordDifferenceAsync"]);

  C({ id: "OR.Comparison", name: "ComparisonOrchestrationService", project: "core", layer: "orchestration", col: 5,
      methods: ["CompareAsync"],
      description: "Splits both bundles by resourceType, asks the matcher registry for a strategy per type, then walks matched pairs property by property after applying the ignore rules. Unmatched resources and types with no matcher become manual-review-required diffs. NOTE: MatchAsync / GetMatchKeyAsync are invoked on whichever IResourceMatcherService GetMatcherAsync returned, so those calls are not drawn as separate edges — follow GetMatcherAsync to see the candidates." });
  D(["OR.Comparison", "CompareAsync"], ["PR.ResourceMatcher", "GetMatcherAsync"]);
  D(["OR.Comparison", "CompareAsync"], ["PR.ListEntryComparison", "CompareListEntryCountsAsync"]);
  for (const [name] of IGNORE_RULES) {
    D(["OR.Comparison", "CompareAsync"], ["PR.Rule." + name, "ShouldIgnoreAsync"]);
    D(["OR.Comparison", "CompareAsync"], ["PR.Rule." + name, "GetReplacementAsync"]);
  }
  D(["OR.Comparison", "CompareAsync"], ["FS.JsonElement", "CreateObjectElement"]);
  D(["OR.Comparison", "CompareAsync"], ["FS.JsonElement", "CreateArrayElement"]);

  /* ==================================================================
     LondonFhirService.Core — coordinations.
     ================================================================== */
  C({ id: "CO.Stu3Patient", name: "Stu3PatientCoordinationService", project: "core", layer: "coordination", col: 4,
      methods: ["GetStructuredRecordSerialisedAsync"],
      description: "Mints the correlation id for the whole request, audits each stage and times it as a metric span, delegates to the patient orchestration for the access-gated fan-out, then hands the per-provider bundles to the reconciliation orchestration, which returns the single serialised bundle the API hands back." });
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditAndMetricBroker", "LogInformationAsync"]);
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditAndMetricBroker", "LogMetricAsync"]);
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"]);
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["OR.Stu3FhirReconciliation", "ReconcileSerialisedAsync"]);

  C({ id: "CO.Comparison", name: "ComparisonCoordinationService", project: "core", layer: "coordination", col: 4,
      methods: ["ProcessFhirRecordsAsync"],
      description: "Drains the compare queue: claim a pair, compare, persist the FhirRecordDifference, then mark both records Completed. A pair with no primary record is marked Failed without comparison; a comparison that throws marks the secondary Failed and still completes the primary." });
  D(["CO.Comparison", "ProcessFhirRecordsAsync"], ["OR.CompareQueue", "GetUnprocessedRecordAsync"]);
  D(["CO.Comparison", "ProcessFhirRecordsAsync"], ["OR.Comparison", "CompareAsync"]);
  D(["CO.Comparison", "ProcessFhirRecordsAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["CO.Comparison", "ProcessFhirRecordsAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["CO.Comparison", "ProcessFhirRecordsAsync"], ["OR.CompareQueue", "PersistFhirRecordDifferencesAsync"]);
  D(["CO.Comparison", "ProcessFhirRecordsAsync"], ["OR.CompareQueue", "ChangeFhirRecordStatusAsync"]);
  D(["CO.Comparison", "ProcessFhirRecordsAsync"], ["LoggingBroker", "LogWarningAsync"]);

  /* ==================================================================
     LondonFhirService.Api — the consumer-facing host.

     The admin CRUD controllers no longer live here: audit rows carry
     whole patient payloads, so every one of them moved to the
     management host, which is reachable only from the business IP
     range. What is left is the headline endpoint, the two config
     endpoints, and the background work.
     ================================================================== */
  C({ id: "API.Patient", name: "PatientController (STU3)", project: "api", layer: "exposer", col: 3,
      methods: ["POST /api/STU3/Patient/$getstructuredrecord"],
      description: "The solution's headline endpoint. Accepts a FHIR Parameters body (custom FhirJson input/output formatters) and returns the reconciled STU3 bundle. Wrapped in a 130-second request-timeout policy." });
  D(["API.Patient", "POST /api/STU3/Patient/$getstructuredrecord"], ["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"]);

  C({ id: "API.ComparisonWorker", name: "ComparisonWorker", project: "api", layer: "exposer", col: 3,
      methods: ["ExecuteAsync"],
      description: "BackgroundService registered by AddBackgroundWorkers. Creates a DI scope per pass, drains the compare queue, then sleeps ComparisonWorkerSettings.SleepIntervalSeconds. This is the only entry point into the comparison half of the solution." });
  D(["API.ComparisonWorker", "ExecuteAsync"], ["CO.Comparison", "ProcessFhirRecordsAsync"]);

  C({ id: "API.MetricPurgeWorker", name: "MetricPurgeWorker", project: "api", layer: "exposer", col: 3,
      methods: ["ExecuteAsync"],
      description: "Runs the metric retention sweep. The purge existed since metrics were added but nothing ever called it, so the table only ever grew — and it takes a row per span rather than per request. Whether anything is deleted is still governed by IsPurgingAllowed and RetentionPeriodInDays; this worker only decides when to ask. A scope per sweep, because the broker beneath the service resolves a scoped client. It goes through the metric foundation service rather than the broker directly, so a failed sweep is reported in this application's exception types." });
  D(["API.MetricPurgeWorker", "ExecuteAsync"], ["FS.Metric", "PurgeMetricsOlderThanRetentionPeriodAsync"]);

  C({ id: "API.Dispatcher", name: "AuditAndMetricsDispatcher", project: "api", layer: "exposer", col: 3,
      methods: ["TryDispatch", "Complete"],
      description: "Where a deferred audit or metric write goes. The library defers these so recording does not add to the elapsed time of the work being recorded, but how that deferral happens is the host's business: a host with a lifecycle can queue the work and drain it under control, which a library with no lifecycle of its own cannot. Rejection is a return value rather than an exception, because the caller is recording telemetry and a full queue must not take down the requests it is trying to measure. Registered as a singleton and shared with the drain worker. Without a host-supplied dispatcher the library falls back to ThreadPoolDispatcher — one work item per write, unbounded, with nothing draining it on shutdown." });

  C({ id: "API.DispatchWorker", name: "AuditAndMetricsDispatchWorker", project: "api", layer: "exposer", col: 3,
      methods: ["ExecuteAsync", "StopAsync"],
      description: "Drains the bounded dispatcher queue. StopAsync completes the queue and waits for in-flight writes, so a shutdown does not silently drop audits that were already accepted." });
  D(["API.DispatchWorker", "ExecuteAsync"], ["API.Dispatcher", "Complete"]);

  C({ id: "API.MetricTelemetryPublisher", name: "MetricTelemetryPublisher", project: "api", layer: "exposer", col: 3,
      methods: ["ExecuteAsync", "Dispose"],
      description: "Subscribes to the metric library's ActivitySource by name and forwards completed spans to Application Insights. Nothing was subscribed before this existed, so every span the library published was dropped. The source name comes from the same bound AuditAndMetricsConfigurations instance the client uses, so the two cannot drift apart." });
  D(["API.MetricTelemetryPublisher", "ExecuteAsync"], ["EXT.ActivitySource", "ActivityListener"]);

  C({ id: "API.Features", name: "FeaturesController", project: "api", layer: "exposer", col: 3,
      methods: ["GET /api/Features"],
      description: "Reads the Features array straight from IConfiguration. No Core dependency." });
  C({ id: "API.FrontendConfigurations", name: "FrontendConfigurationsController", project: "api", layer: "exposer", col: 3,
      methods: ["GET /api/FrontendConfigurations"],
      description: "Anonymous endpoint returning the FrontendConfiguration section (clientId / authority / scopes) so a SPA can build its MSAL config before sign-in. No Core dependency." });

  /* ==================================================================
     LondonFhirService.Manage — the management host + its React SPA.

     Every admin CRUD controller lives here. All of them are
     [Authorize(Roles = "Administrators,Users")]; the audit and metric
     controllers additionally mark their write verbs [InvisibleApi],
     which the middleware on this host enforces by answering 404 without
     the key header — so those verbs exist to let the acceptance suite
     seed and tear down a database rather than as an operator-facing way
     to rewrite compliance records or telemetry.
     ================================================================== */
  const CRUD_ROUTES = (entity, route, idParam) => ({
    [`POST /api/${route}`]: `Add${entity}Async`,
    [`GET /api/${route}`]: `RetrieveAll${entity}sAsync`,
    [`GET /api/${route}/{${idParam}}`]: `Retrieve${entity}ByIdAsync`,
    [`PUT /api/${route}`]: `Modify${entity}Async`,
    [`DELETE /api/${route}/{${idParam}}`]: `Remove${entity}ByIdAsync`,
  });

  const restControllers = [
    { id: "MG.Audits", name: "AuditsController", project: "manage", svc: "FS.Audit", entity: "Audit",
      route: "Audits", idParam: "auditId", invisible: true,
      overrides: { "POST /api/Audits": "AddAuditAsync(audit)", "GET /api/Audits": "RetrieveAllAuditsAsync" } },
    { id: "MG.Metrics", name: "MetricsController", project: "manage", svc: "FS.Metric", entity: "Metric",
      route: "Metrics", idParam: "metricId", invisible: true, noUpdate: true,
      overrides: { "GET /api/Metrics": "RetrieveAllMetricsAsync" } },
    { id: "MG.Providers", name: "ProvidersController", project: "manage", svc: "FS.Provider", entity: "Provider",
      route: "Providers", idParam: "providerId", adminWrites: true,
      overrides: { "GET /api/Providers": "RetrieveAllProvidersAsync" } },
    { id: "MG.FhirRecords", name: "FhirRecordsController", project: "manage", svc: "FS.FhirRecord", entity: "FhirRecord",
      route: "FhirRecords", idParam: "fhirRecordId" },
    { id: "MG.FhirRecordDifferences", name: "FhirRecordDifferencesController", project: "manage",
      svc: "FS.FhirRecordDifference", entity: "FhirRecordDifference",
      route: "FhirRecordDifferences", idParam: "fhirRecordDifferenceId" },
  ];

  for (const rc of restControllers) {
    const map = CRUD_ROUTES(rc.entity, rc.route, rc.idParam);
    if (rc.noUpdate) delete map[`PUT /api/${rc.route}`];
    Object.assign(map, rc.overrides || {});
    const hidden = rc.invisible
      ? " Create, update and delete carry [InvisibleApi]: the middleware on this host makes them unroutable without the key header, so they answer 404 to anyone who does not hold it."
      : "";
    const noUpdate = rc.noUpdate
      ? " There is no PUT — a metric records work that already happened, so the table is append-only and no update path exists beneath the controller to expose."
      : "";
    const adminWrites = rc.adminWrites
      ? " Reads are open to Administrators and Users; create, update and delete narrow to Administrators with a second [Authorize] on the method, because a provider row decides who the patient fan-out calls and which source is primary. Nothing here is [InvisibleApi] — providers are configuration an operator manages, not a record only the acceptance suite should seed."
      : "";
    C({ id: rc.id, name: rc.name, project: rc.project, layer: "exposer", col: 3, methods: Object.keys(map),
        description: `RESTFulController over ${rc.entity}, [Authorize(Roles = "Administrators,Users")]. The list endpoint is OData-enabled ([EnableQuery], PageSize 5000 in DEBUG and 50 otherwise) and returns the IQueryable straight from the foundation service, and the entity is registered in the host's EDM model so it is reachable on /odata as well as /api. Xeption types are mapped one-for-one onto RFC problem responses.${hidden}${noUpdate}${adminWrites}` });
    for (const [route, call] of Object.entries(map)) D([rc.id, route], [rc.svc, call]);
  }

  C({ id: "MG.Features", name: "FeaturesController", project: "manage", layer: "exposer", col: 3,
      methods: ["GET /api/Features"],
      description: "[Authorize]'d read of the Features array from IConfiguration." });
  C({ id: "MG.FrontendConfigurations", name: "FrontendConfigurationsController", project: "manage", layer: "exposer", col: 3,
      methods: ["GET /api/FrontendConfigurations"],
      description: "Anonymous endpoint the SPA calls before MSAL exists, to learn its clientId / authority / scopes." });

  C({ id: "MC.Bootstrap", name: "main.tsx → MsalConfig.build", project: "manage-client", layer: "exposer", col: 0,
      methods: ["MsalConfig.build", "PublicClientApplication.initialize"],
      description: "The SPA bootstraps by fetching its own MSAL configuration from the Manage host, then instantiating MSAL and rendering App." });
  C({ id: "MC.FeatureSwitch", name: "FeatureSwitch", project: "manage-client", layer: "exposer", col: 0,
      methods: ["FeatureSwitch"],
      description: "Renders its children only when the named feature is in the host's Features array." });

  C({ id: "MC.FeatureService", name: "featureService", project: "manage-client", layer: "view", col: 1,
      methods: ["useGetAllFeatures"],
      description: "TanStack Query wrapper (staleTime: Infinity) over the feature broker. Still the SPA's only foundation service — the admin CRUD surfaces are exposed by the host but not yet consumed by the client." });
  D(["MC.FeatureSwitch", "FeatureSwitch"], ["MC.FeatureService", "useGetAllFeatures"]);

  C({ id: "MC.FeatureBroker", name: "apiBroker.features", project: "manage-client", layer: "broker", col: 2,
      methods: ["GetAllFeatureAsync"] });
  C({ id: "MC.FrontendConfigurationBroker", name: "apiBroker.frontendConfiguration", project: "manage-client", layer: "broker", col: 2,
      methods: ["GetFrontendConfigruationAsync"],
      description: "Calls axios directly rather than through ApiBroker — it runs before MSAL is configured, so there is no token to attach." });
  C({ id: "MC.ApiBroker", name: "apiBroker", project: "manage-client", layer: "broker", col: 2,
      methods: ["GetAsync", "GetAsyncAbsolute", "PostAsync", "PostFormAsync", "PutAsync", "DeleteAsync"],
      description: "axios wrapper that acquires an MSAL token silently (falling back to a redirect on InteractionRequiredAuthError) and attaches it as a bearer header." });

  D(["MC.FeatureService", "useGetAllFeatures"], ["MC.FeatureBroker", "GetAllFeatureAsync"]);
  D(["MC.FeatureBroker", "GetAllFeatureAsync"], ["MC.ApiBroker", "GetAsync"]);
  D(["MC.ApiBroker", "GetAsync"], ["MG.Features", "GET /api/Features"]);
  D(["MC.Bootstrap", "MsalConfig.build"], ["MC.FrontendConfigurationBroker", "GetFrontendConfigruationAsync"]);
  D(["MC.FrontendConfigurationBroker", "GetFrontendConfigruationAsync"], ["MG.FrontendConfigurations", "GET /api/FrontendConfigurations"]);

  /* ==================================================================
     roots — tree order controls the vertical layout
     ================================================================== */
  roots.push(
    // LondonFhirService.Manage.Client (pulls the two Manage config endpoints in)
    "MC.Bootstrap", "MC.FeatureSwitch",
    // LondonFhirService.Api
    "API.Patient", "API.ComparisonWorker", "API.MetricPurgeWorker", "API.DispatchWorker",
    "API.Dispatcher", "API.MetricTelemetryPublisher", "API.Features", "API.FrontendConfigurations",
    // LondonFhirService.Manage
    "MG.Audits", "MG.Metrics", "MG.Providers", "MG.FhirRecords", "MG.FhirRecordDifferences",
    // LondonFhirService.Core — every service gets a full-surface tree of its own
    "CO.Stu3Patient", "CO.Comparison",
    "OR.Stu3Patient", "OR.Stu3FhirReconciliation", "OR.CompareQueue", "OR.Comparison",
    "PR.ResourceMatcher", "PR.ListEntryComparison",
    ...IGNORE_RULES.map(([name]) => "PR.Rule." + name),
    "FS.Audit", "FS.Metric", "FS.ConsumerAccess", ...CRUD_ENTITIES.map(c => "FS." + c.e),
    "FS.Stu3Patient", "FS.JsonElement",
    ...MATCHERS.map(([resource]) => "FS.Matcher." + resource),
    "AuditAndMetricBroker", "AuditAndMetricStorageBroker", "AuditUserBroker",
    "SecurityBroker", "SecurityAuditBroker", "StorageBroker", "StorageBrokerFactory",
    "Stu3FhirBroker", "ConsumerAccessBroker",
    "DateTimeBroker", "IdentifierBroker", "LoggingBroker",
    // Core.Abstractions — the ports the library declares
    "ABS.Ports",
    // the audit and metrics library
    "LIB.AMClient", "LIB.AM.AuditClient", "LIB.AM.MetricClient",
    "LIB.AM.AuditService", "LIB.AM.MetricService", "LIB.AM.MetricBroker",
    // client libraries (single shared copies)
    "LIB.SecurityClient", "LIB.EFCoreClient", "LIB.FhirAbstractionProvider", "LIB.FhirProvider",
    // externals
    "EXT.EFCore", "EXT.ILogger", "EXT.ActivitySource", "EXT.TokenCredential", "EXT.HttpClient",
    "EXT.DdsStu3", "EXT.LdsStu3",
  );

  /* ------------------------------------------------------------------
     Externals show exactly the public surface this solution calls.
     Derive their method rows from the declared edges so the rows and
     the arrows can never drift apart.
     ------------------------------------------------------------------ */
  for (const extId of ["EXT.EFCore", "EXT.ILogger", "EXT.ActivitySource", "EXT.TokenCredential",
                       "EXT.HttpClient", "EXT.DdsStu3", "EXT.LdsStu3", "LIB.FhirAbstractionProvider"]) {
    const comp = components.find(c => c.id === extId);
    const called = [];
    for (const e of edges) {
      if (e.kind === "direct" && e.to[0] === extId && e.to[1] && !called.includes(e.to[1])) called.push(e.to[1]);
    }
    comp.methods = called.sort((a, b) => a.localeCompare(b));
  }

  window.LFS_DATA = {
    projects,
    components,
    events,
    edges,
    roots,
    eventBrokerId: null,
  };
})();
