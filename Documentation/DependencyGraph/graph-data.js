/* =====================================================================
   London FHIR Service solution dependency data — consumed by index.html
   (both the single-copy and the per-consumer view).

   Hand-maintained model of the solution's components and flows,
   generated from the actual source (2026-08-11). The 7 uniform CRUD
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
    { id: "pkg-security", name: "ISL.Security.Client", kind: "library" },
    { id: "pkg-efcore", name: "STX.EFCore.Client", kind: "library" },
    { id: "pkg-fhir", name: "LondonFhirService.Providers.FHIR.STU3", kind: "library" },
    { id: "ext-efcore", name: "EF Core / SQL Server", kind: "external" },
    { id: "ext-logging", name: "Microsoft.Extensions.Logging", kind: "external" },
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
      description: "The DbContext handed to STX.EFCore.Client's EFCoreClient. In this solution that is Core's StorageBroker itself — it derives from EFxceptionsContext (EF Core DbContext) and passes `this` into `new EFCoreClient(this)`. SQL Server with HierarchyId enabled." });
  C({ id: "EXT.ILogger", name: "ILogger<LoggingBroker>", project: "ext-logging", layer: "external", col: 12, shared: true, methods: [],
      description: "Microsoft.Extensions.Logging. The only sink LoggingBroker writes to; Application Insights is wired at host level." });
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
      description: "STX.EFCore.Client 3.0.0. Every StorageBroker partial funnels through these eight generic primitives; the broker constructs it with itself as the DbContext." });

  C({ id: "LIB.FhirAbstractionProvider", name: "FhirAbstractionProvider", project: "pkg-fhir", layer: "client", col: 10, shared: true, methods: [],
      description: "LondonFhirService.Providers.FHIR.STU3.Abstractions. Registered as a singleton wrapping the DDS and LDS providers; Stu3FhirBroker forwards its whole typed-resource surface. Method rows are derived from the declared edges." });

  C({ id: "LIB.FhirProvider", name: "IFhirProvider", project: "pkg-fhir", layer: "client", col: 11, shared: true,
      methods: ["ProviderName", "DisplayName", "SupportsResource", "Patients.GetStructuredRecordSerialisedAsync"],
      description: "The per-provider surface Stu3PatientService actually calls, resolved out of the FhirProviders collection by matching Provider.FullyQualifiedName. Two implementations are registered: DdsStu3Provider and LdsStu3Provider." });

  D(["LIB.FhirAbstractionProvider", "FhirProviders"], ["LIB.FhirProvider", null]);
  for (const impl of ["EXT.DdsStu3", "EXT.LdsStu3"])
    D(["LIB.FhirProvider", "Patients.GetStructuredRecordSerialisedAsync"], [impl, "Patients.GetStructuredRecordSerialisedAsync"]);

  /* ==================================================================
     LondonFhirService.Core — brokers.
     ================================================================== */
  C({ id: "StorageBroker", name: "StorageBroker", project: "core", layer: "broker", col: 8,
      methods: [],
      description: "Partial EF Core DbContext (derives from EFxceptions' EFxceptionsContext) with one partial per entity. Every method delegates to a private generic that calls STX.EFCore.Client's EFCoreClient. Registered both as a DbContextFactory and a scoped IStorageBroker; OnConfiguring reads the LondonFhirServiceConnectionString and enables HierarchyId. Per-entity method rows are generated from the entity config." });

  C({ id: "StorageBrokerFactory", name: "StorageBrokerFactory", project: "core", layer: "broker", col: 8,
      methods: ["CreateStorageBrokerAsync"],
      description: "Wraps IDbContextFactory<StorageBroker>. AuditService and Stu3PatientService use it to get a short-lived, independently-disposed StorageBroker instead of the scoped one — they write from background/parallel work where the request-scoped context is unsafe." });
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

  C({ id: "AuditBroker", name: "AuditBroker", project: "core", layer: "broker", col: 8,
      methods: ["BulkLogAsync", "LogAsync", "LogInformationAsync", "LogWarningAsync", "LogErrorAsync", "LogCriticalAsync"],
      description: "The business-audit trail (persisted Audit rows), distinct from LoggingBroker's ILogger output. Every method funnels into AuditClient, which calls AuditService — so an audit write travels broker → client → foundation service → storage." });
  for (const m of ["LogAsync", "LogInformationAsync", "LogWarningAsync", "LogErrorAsync", "LogCriticalAsync"])
    D(["AuditBroker", m], ["AuditClient", "LogAuditAsync"]);
  D(["AuditBroker", "BulkLogAsync"], ["AuditClient", "BulkLogAuditsAsync"]);

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
     LondonFhirService.Core — clients.
     ================================================================== */
  C({ id: "AuditClient", name: "AuditClient", project: "core", layer: "client", col: 9,
      methods: ["LogAuditAsync", "BulkLogAuditsAsync"],
      description: "Sits between AuditBroker and AuditService — the one place in the solution where a broker calls back into a foundation service. The AuditService copy drawn to its left is that call." });
  D(["AuditClient", "LogAuditAsync"], ["FS.Audit", "AddAuditAsync(auditType, title, message, …)"]);
  D(["AuditClient", "BulkLogAuditsAsync"], ["FS.Audit", "BulkAddAuditsAsync"]);

  /* ==================================================================
     LondonFhirService.Core — foundation services.

     Seven entities share one CRUD template; three read/write variants:
       A audited CRUD — security-audit stamps on add/modify, hard delete
       B audited CRUD — as A, plus a remove-audit stamp + Update before
         the Delete (soft-delete values are persisted first)
       C plain CRUD  — no security-audit broker at all
     ================================================================== */
  const CRUD_ENTITIES = [
    { e: "FhirRecord", plural: "FhirRecords", variant: "A" },
    { e: "FhirRecordDifference", plural: "FhirRecordDifferences", variant: "A" },
    { e: "Provider", plural: "Providers", variant: "A" },
  ];

  // every entity gets the same StorageBroker surface
  const STORAGE_ENTITIES = CRUD_ENTITIES.concat([{ e: "Audit", plural: "Audits", bulk: true }]);
  const storageBroker = components.find(c => c.id === "StorageBroker");
  for (const cfg of STORAGE_ENTITIES) {
    const rows = [];
    if (cfg.bulk) rows.push(`BulkInsert${cfg.plural}Async`);
    rows.push(`Insert${cfg.e}Async`, `SelectAll${cfg.plural}Async`, `Select${cfg.e}ByIdAsync`,
              `Update${cfg.e}Async`, `Delete${cfg.e}Async`);
    storageBroker.methods.push(...rows);

    if (cfg.bulk) D(["StorageBroker", `BulkInsert${cfg.plural}Async`], ["LIB.EFCoreClient", "BulkInsertAsync"]);
    D(["StorageBroker", `Insert${cfg.e}Async`], ["LIB.EFCoreClient", "InsertAsync"]);
    D(["StorageBroker", `SelectAll${cfg.plural}Async`], ["LIB.EFCoreClient", "SelectAllAsync"]);
    D(["StorageBroker", `Select${cfg.e}ByIdAsync`], ["LIB.EFCoreClient", "SelectAsync"]);
    D(["StorageBroker", `Update${cfg.e}Async`], ["LIB.EFCoreClient", "UpdateAsync"]);
    D(["StorageBroker", `Delete${cfg.e}Async`], ["LIB.EFCoreClient", "DeleteAsync"]);
  }
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

  /* -- ConsumerAccessService: a pure passthrough over the remote
        consumer-access API, so it owns no storage surface at all -- */
  C({ id: "FS.ConsumerAccess", name: "ConsumerAccessService", project: "core", layer: "foundation", col: 7,
      methods: ["CheckConsumerAccessAsync"],
      description: "A single passthrough onto ConsumerAccessBroker, consumed by Stu3PatientOrchestrationService.ValidateAccess. Validates the ValidateAccessRequest (ConsumerUserId, NhsNumber, CorrelationId) and forwards it with the caller's CancellationToken; its TryCatch maps HttpRequestException to a critical dependency exception and timeout / cancellation onto TimedOut- and CancelledConsumerAccessServiceException. It takes no storage broker — consumer access is no longer held locally." });
  D(["FS.ConsumerAccess", "CheckConsumerAccessAsync"], ["ConsumerAccessBroker", "CheckConsumerAccessAsync"]);

  /* -- AuditService: the one CRUD service that deviates structurally -- */
  const AUDIT_ADD_ARGS = "AddAuditAsync(auditType, title, message, …)";
  C({ id: "FS.Audit", name: "AuditService", project: "core", layer: "foundation", col: 7,
      methods: [AUDIT_ADD_ARGS, "AddAuditAsync(audit)", "BulkAddAuditsAsync", "RetrieveAllAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync"],
      description: "The business-audit trail. Unlike every other foundation service it takes IStorageBrokerFactory as well as the scoped IStorageBroker: every write and every read-by-id opens (and disposes) its own StorageBroker, because audits are written from parallel provider calls and background work. Only RetrieveAllAuditsAsync uses the injected scoped broker. BulkAddAuditsAsync batches (default 10 000) through BatchBulkAddAuditsAsync, minting id + audit values per row and swallowing per-row failures into LoggingBroker." });
  for (const m of [AUDIT_ADD_ARGS, "AddAuditAsync(audit)", "BulkAddAuditsAsync", "RetrieveAuditByIdAsync", "ModifyAuditAsync", "RemoveAuditByIdAsync"])
    D(["FS.Audit", m], ["StorageBrokerFactory", "CreateStorageBrokerAsync"]);
  D(["FS.Audit", AUDIT_ADD_ARGS], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["FS.Audit", AUDIT_ADD_ARGS], ["SecurityAuditBroker", "GetUserIdAsync"]);
  D(["FS.Audit", AUDIT_ADD_ARGS], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["FS.Audit", AUDIT_ADD_ARGS], ["StorageBroker", "InsertAuditAsync"]);
  D(["FS.Audit", "AddAuditAsync(audit)"], ["SecurityAuditBroker", "ApplyAddAuditValuesAsync"]);
  D(["FS.Audit", "AddAuditAsync(audit)"], ["SecurityAuditBroker", "GetUserIdAsync"]);
  D(["FS.Audit", "AddAuditAsync(audit)"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["FS.Audit", "AddAuditAsync(audit)"], ["StorageBroker", "InsertAuditAsync"]);
  D(["FS.Audit", "BulkAddAuditsAsync"], ["SecurityAuditBroker", "GetUserIdAsync"]);
  D(["FS.Audit", "BulkAddAuditsAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["FS.Audit", "BulkAddAuditsAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["FS.Audit", "BulkAddAuditsAsync"], ["StorageBroker", "BulkInsertAuditsAsync"]);
  D(["FS.Audit", "RetrieveAllAuditsAsync"], ["StorageBroker", "SelectAllAuditsAsync"]);
  D(["FS.Audit", "RetrieveAuditByIdAsync"], ["StorageBroker", "SelectAuditByIdAsync"]);
  D(["FS.Audit", "ModifyAuditAsync"], ["SecurityAuditBroker", "ApplyModifyAuditValuesAsync"]);
  D(["FS.Audit", "ModifyAuditAsync"], ["SecurityAuditBroker", "GetUserIdAsync"]);
  D(["FS.Audit", "ModifyAuditAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["FS.Audit", "ModifyAuditAsync"], ["StorageBroker", "SelectAuditByIdAsync"]);
  D(["FS.Audit", "ModifyAuditAsync"], ["StorageBroker", "UpdateAuditAsync"]);
  D(["FS.Audit", "RemoveAuditByIdAsync"], ["StorageBroker", "SelectAuditByIdAsync"]);
  D(["FS.Audit", "RemoveAuditByIdAsync"], ["SecurityAuditBroker", "ApplyRemoveAuditValuesAsync"]);
  D(["FS.Audit", "RemoveAuditByIdAsync"], ["SecurityAuditBroker", "GetUserIdAsync"]);
  D(["FS.Audit", "RemoveAuditByIdAsync"], ["StorageBroker", "UpdateAuditAsync"]);
  D(["FS.Audit", "RemoveAuditByIdAsync"], ["StorageBroker", "DeleteAuditAsync"]);

  /* -- STU3 patient retrieval -- */
  C({ id: "FS.Stu3Patient", name: "Stu3PatientService", project: "core", layer: "foundation", col: 7,
      methods: ["GetStructuredRecordSerialisedAsync"],
      description: "Fans out to every active provider in parallel (Task.WhenAll), each call wrapped in a linked CancellationTokenSource that cancels after PatientServiceConfig.MaxProviderWaitTimeMilliseconds. Each returned bundle is persisted as a Pending FhirRecord (via its own factory-created StorageBroker) and audited under STU3-Patient-GetStructuredRecordSerialised — including a -DATA entry carrying the raw JSON. Providers that do not support Patients/$GetStructuredRecord are dropped before the fan-out." });
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditBroker", "LogInformationAsync"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["Stu3FhirBroker", "FhirProviders"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "ProviderName"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "SupportsResource"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "DisplayName"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LIB.FhirProvider", "Patients.GetStructuredRecordSerialisedAsync"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["SecurityAuditBroker", "ApplyAddAuditValuesAsync"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["StorageBrokerFactory", "CreateStorageBrokerAsync"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["StorageBroker", "InsertFhirRecordAsync"]);
  D(["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["LoggingBroker", "LogInformationAsync"]);

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
      methods: ["GetStructuredRecordSerialisedAsync", "ValidateAccess"],
      description: "Gates the request on access, then fans it out. GetStructuredRecordSerialisedAsync runs the access check first (through the same private helper ValidateAccess wraps, so the exception is localised once), reads the STU3 providers, keeps the active ones (IsActive plus the ActiveFrom/ActiveTo window) with the primary first, validates that exactly one primary exists, and returns a StructuredRecordsResponse (primary provider + per-provider bundles) for the coordination service to reconcile. ValidateAccess honours AccessConfigurations.CheckAccessPermissions: off, it audits the skip and returns; on, it resolves the caller, builds a ValidateAccessRequest and asks ConsumerAccessService — not allowed writes an Access Forbidden audit carrying the returned reason codes and throws ForbiddenPatientOrchestrationException, allowed writes an Access Allowed audit naming the organisations that granted it." });
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditBroker", "LogInformationAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["SecurityBroker", "GetCurrentUserAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["FS.ConsumerAccess", "CheckConsumerAccessAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["FS.Provider", "RetrieveAllProvidersAsync"]);
  D(["OR.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["FS.Stu3Patient", "GetStructuredRecordSerialisedAsync"]);
  D(["OR.Stu3Patient", "ValidateAccess"], ["AuditBroker", "LogInformationAsync"]);
  D(["OR.Stu3Patient", "ValidateAccess"], ["SecurityBroker", "GetCurrentUserAsync"]);
  D(["OR.Stu3Patient", "ValidateAccess"], ["FS.ConsumerAccess", "CheckConsumerAccessAsync"]);

  C({ id: "OR.CompareQueue", name: "CompareQueueOrchestrationService", project: "core", layer: "orchestration", col: 5,
      methods: ["GetUnprocessedRecordAsync", "ChangeFhirRecordStatusAsync", "PersistFhirRecordDifferencesAsync"],
      description: "The comparison queue. GetUnprocessedRecordAsync claims the oldest Pending secondary FhirRecord that has been settled for at least 5 minutes, flips it to Processing, and pairs it with the primary record sharing its CorrelationId." });
  D(["OR.CompareQueue", "GetUnprocessedRecordAsync"], ["DateTimeBroker", "GetCurrentDateTimeOffsetAsync"]);
  D(["OR.CompareQueue", "GetUnprocessedRecordAsync"], ["FS.FhirRecord", "RetrieveAllFhirRecordsAsync"]);
  D(["OR.CompareQueue", "GetUnprocessedRecordAsync"], ["FS.FhirRecord", "ModifyFhirRecordAsync"]);
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
      description: "Mints the correlation id for the whole request, audits each stage, delegates to the patient orchestration for the access-gated fan-out, then hands the per-provider bundles to the reconciliation orchestration, which returns the single serialised bundle the API hands back." });
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["IdentifierBroker", "GetIdentifierAsync"]);
  D(["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"], ["AuditBroker", "LogInformationAsync"]);
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
     ================================================================== */
  const CRUD_ROUTES = (entity, route, idParam) => ({
    [`POST /api/${route}`]: `Add${entity}Async`,
    [`GET /api/${route}`]: `RetrieveAll${entity}sAsync`,
    [`GET /api/${route}/{${idParam}}`]: `Retrieve${entity}ByIdAsync`,
    [`PUT /api/${route}`]: `Modify${entity}Async`,
    [`DELETE /api/${route}/{${idParam}}`]: `Remove${entity}ByIdAsync`,
  });

  const restControllers = [
    { id: "API.Audits", name: "AuditsController", project: "api", svc: "FS.Audit", entity: "Audit",
      route: "Audits", idParam: "auditId",
      overrides: { "POST /api/Audits": "AddAuditAsync(audit)", "GET /api/Audits": "RetrieveAllAuditsAsync" } },
    { id: "API.FhirRecords", name: "FhirRecordsController", project: "api", svc: "FS.FhirRecord", entity: "FhirRecord",
      route: "FhirRecords", idParam: "fhirRecordId" },
    { id: "API.FhirRecordDifferences", name: "FhirRecordDifferencesController", project: "api",
      svc: "FS.FhirRecordDifference", entity: "FhirRecordDifference",
      route: "FhirRecordDifferences", idParam: "fhirRecordDifferenceId" },
    { id: "MG.FhirRecords", name: "FhirRecordsController", project: "manage", svc: "FS.FhirRecord", entity: "FhirRecord",
      route: "FhirRecords", idParam: "fhirRecordId" },
    { id: "MG.FhirRecordDifferences", name: "FhirRecordDifferencesController", project: "manage",
      svc: "FS.FhirRecordDifference", entity: "FhirRecordDifference",
      route: "FhirRecordDifferences", idParam: "fhirRecordDifferenceId" },
  ];

  for (const rc of restControllers) {
    const map = CRUD_ROUTES(rc.entity, rc.route, rc.idParam);
    Object.assign(map, rc.overrides || {});
    C({ id: rc.id, name: rc.name, project: rc.project, layer: "exposer", col: 3, methods: Object.keys(map),
        description: `RESTFulController over ${rc.entity}. The list endpoint is OData-enabled ([EnableQuery], PageSize 5000 in DEBUG and 50 otherwise) and returns the IQueryable straight from the foundation service. Writes are marked [InvisibleApi]. Xeption types are mapped one-for-one onto RFC problem responses.` });
    for (const [route, call] of Object.entries(map)) D([rc.id, route], [rc.svc, call]);
  }

  C({ id: "API.Patient", name: "PatientController (STU3)", project: "api", layer: "exposer", col: 3,
      methods: ["POST /api/STU3/Patient/$getstructuredrecord"],
      description: "The solution's headline endpoint. Accepts a FHIR Parameters body (custom FhirJson input/output formatters) and returns the reconciled STU3 bundle. Wrapped in a 130-second request-timeout policy." });
  D(["API.Patient", "POST /api/STU3/Patient/$getstructuredrecord"], ["CO.Stu3Patient", "GetStructuredRecordSerialisedAsync"]);

  C({ id: "API.ComparisonWorker", name: "ComparisonWorker", project: "api", layer: "exposer", col: 3,
      methods: ["ExecuteAsync"],
      description: "BackgroundService registered by AddBackgroundWorkers. Creates a DI scope per pass, drains the compare queue, then sleeps ComparisonWorkerSettings.SleepIntervalSeconds. This is the only entry point into the comparison half of the solution." });
  D(["API.ComparisonWorker", "ExecuteAsync"], ["CO.Comparison", "ProcessFhirRecordsAsync"]);

  C({ id: "API.Features", name: "FeaturesController", project: "api", layer: "exposer", col: 3,
      methods: ["GET /api/Features"],
      description: "Reads the Features array straight from IConfiguration. No Core dependency." });
  C({ id: "API.FrontendConfigurations", name: "FrontendConfigurationsController", project: "api", layer: "exposer", col: 3,
      methods: ["GET /api/FrontendConfigurations"],
      description: "Anonymous endpoint returning the FrontendConfiguration section (clientId / authority / scopes) so a SPA can build its MSAL config before sign-in. No Core dependency." });

  /* ==================================================================
     LondonFhirService.Manage — the management host + its React SPA.
     ================================================================== */
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
      description: "TanStack Query wrapper (staleTime: Infinity) over the feature broker." });
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
    "API.Patient", "API.ComparisonWorker", "API.Audits", "API.FhirRecords",
    "API.FhirRecordDifferences", "API.Features", "API.FrontendConfigurations",
    // LondonFhirService.Manage
    "MG.FhirRecords", "MG.FhirRecordDifferences",
    // LondonFhirService.Core — every service gets a full-surface tree of its own
    "CO.Stu3Patient", "CO.Comparison",
    "OR.Stu3Patient", "OR.Stu3FhirReconciliation", "OR.CompareQueue", "OR.Comparison",
    "PR.ResourceMatcher", "PR.ListEntryComparison",
    ...IGNORE_RULES.map(([name]) => "PR.Rule." + name),
    "FS.Audit", "FS.ConsumerAccess", ...CRUD_ENTITIES.map(c => "FS." + c.e),
    "FS.Stu3Patient", "FS.JsonElement",
    ...MATCHERS.map(([resource]) => "FS.Matcher." + resource),
    "AuditClient",
    "AuditBroker", "SecurityBroker", "SecurityAuditBroker", "StorageBroker", "StorageBrokerFactory",
    "Stu3FhirBroker", "ConsumerAccessBroker",
    "DateTimeBroker", "IdentifierBroker", "LoggingBroker",
    // client libraries (single shared copies)
    "LIB.SecurityClient", "LIB.EFCoreClient", "LIB.FhirAbstractionProvider", "LIB.FhirProvider",
    // externals
    "EXT.EFCore", "EXT.ILogger", "EXT.TokenCredential", "EXT.HttpClient",
    "EXT.DdsStu3", "EXT.LdsStu3",
  );

  /* ------------------------------------------------------------------
     Externals show exactly the public surface this solution calls.
     Derive their method rows from the declared edges so the rows and
     the arrows can never drift apart.
     ------------------------------------------------------------------ */
  for (const extId of ["EXT.EFCore", "EXT.ILogger", "EXT.TokenCredential", "EXT.HttpClient",
                       "EXT.DdsStu3", "EXT.LdsStu3", "LIB.FhirAbstractionProvider"]) {
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
