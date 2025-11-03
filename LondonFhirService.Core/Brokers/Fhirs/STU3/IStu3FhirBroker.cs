// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Resources;

namespace LondonFhirService.Core.Brokers.Fhirs.STU3
{
    public interface IStu3FhirBroker
    {
        IReadOnlyCollection<IFhirProvider> FhirProviders { get; }
        IAccountResource Accounts(string providerName);
        IActivityDefinitionResource ActivityDefinitions(string providerName);
        IAdverseEventResource AdverseEvents(string providerName);
        IAllergyIntoleranceResource AllergyIntolerances(string providerName);
        IAppointmentResponseResource AppointmentResponses(string providerName);
        IAppointmentResource Appointments(string providerName);
        IAuditEventResource AuditEvents(string providerName);
        IBasicResource Basics(string providerName);
        IBinaryResource Binaries(string providerName);
        IBundleResource Bundles(string providerName);
        ICapabilityStatementResource CapabilityStatements(string providerName);
        ICarePlanResource CarePlans(string providerName);
        ICareTeamResource CareTeams(string providerName);
        IChargeItemResource ChargeItems(string providerName);
        IClaimResponseResource ClaimResponses(string providerName);
        IClaimResource Claims(string providerName);
        IClinicalImpressionResource ClinicalImpressions(string providerName);
        ICodeSystemResource CodeSystems(string providerName);
        ICommunicationRequestResource CommunicationRequests(string providerName);
        ICommunicationResource Communications(string providerName);
        ICompartmentDefinitionResource CompartmentDefinitions(string providerName);
        ICompositionResource Compositions(string providerName);
        IConceptMapResource ConceptMaps(string providerName);
        IConditionResource Conditions(string providerName);
        IConsentResource Consents(string providerName);
        IContractResource Contracts(string providerName);
        ICoverageResource Coverages(string providerName);
        IDetectedIssueResource DetectedIssues(string providerName);
        IDeviceMetricResource DeviceMetrics(string providerName);
        IDeviceRequestResource DeviceRequests(string providerName);
        IDeviceResource Devices(string providerName);
        IDeviceUseStatementResource DeviceUseStatements(string providerName);
        IDiagnosticReportResource DiagnosticReports(string providerName);
        IDocumentManifestResource DocumentManifests(string providerName);
        IDocumentReferenceResource DocumentReferences(string providerName);
        IEncounterResource Encounters(string providerName);
        IEndpointResource Endpoints(string providerName);
        IEnrollmentRequestResource EnrollmentRequests(string providerName);
        IEnrollmentResponseResource EnrollmentResponses(string providerName);
        IEpisodeOfCareResource EpisodeOfCare(string providerName);
        IExplanationOfBenefitResource ExplanationsOfBenefits(string providerName);
        IFamilyMemberHistoryResource FamilyMemberHistories(string providerName);
        IFlagResource Flags(string providerName);
        IGoalResource Goals(string providerName);
        IGraphDefinitionResource GraphDefinitions(string providerName);
        IGroupResource Groups(string providerName);
        IGuidanceResponseResource GuidanceResponses(string providerName);
        IHealthcareServiceResource HealthcareServices(string providerName);
        IImagingStudyResource ImagingStudies(string providerName);
        IImmunizationRecommendationResource ImmunizationRecommendations(string providerName);
        IImmunizationResource Immunizations(string providerName);
        IImplementationGuideResource ImplementationGuides(string providerName);
        ILibraryResource Libraries(string providerName);
        ILinkageResource Linkages(string providerName);
        IListResource Lists(string providerName);
        ILocationResource Locations(string providerName);
        IMeasureReportResource MeasureReports(string providerName);
        IMeasureResource Measures(string providerName);
        IMediaResource Media(string providerName);
        IMedicationAdministrationResource MedicationAdministrations(string providerName);
        IMedicationDispenseResource MedicationDispenses(string providerName);
        IMedicationRequestResource MedicationRequests(string providerName);
        IMedicationResource Medications(string providerName);
        IMedicationStatementResource MedicationStatements(string providerName);
        IMessageDefinitionResource MessageDefinitions(string providerName);
        IMessageHeaderResource MessageHeaders(string providerName);
        INamingSystemResource NamingSystems(string providerName);
        INutritionOrderResource NutritionOrders(string providerName);
        IObservationResource Observations(string providerName);
        IOperationDefinitionResource OperationDefinitions(string providerName);
        IOperationOutcomeResource OperationOutcomes(string providerName);
        IOrganizationResource Organizations(string providerName);
        IParametersResource Parameters(string providerName);
        IPatientResource Patients(string providerName);
        IPaymentNoticeResource PaymentNotices(string providerName);
        IPaymentReconciliationResource PaymentReconciliations(string providerName);
        IPersonResource Persons(string providerName);
        IPlanDefinitionResource PlanDefinitions(string providerName);
        IPractitionerRoleResource PractitionerRoles(string providerName);
        IPractitionerResource Practitioners(string providerName);
        IProcedureResource Procedures(string providerName);
        IProvenanceResource Provenances(string providerName);
        IQuestionnaireResponseResource QuestionnaireResponses(string providerName);
        IQuestionnaireResource Questionnaires(string providerName);
        IRelatedPersonResource RelatedPersons(string providerName);
        IRequestGroupResource RequestGroups(string providerName);
        IResearchStudyResource ResearchStudies(string providerName);
        IResearchSubjectResource ResearchSubjects(string providerName);
        IRiskAssessmentResource RiskAssessments(string providerName);
        IScheduleResource Schedules(string providerName);
        ISearchParameterResource SearchParameters(string providerName);
        ISlotResource Slots(string providerName);
        ISpecimenResource Specimens(string providerName);
        IStructureDefinitionResource StructureDefinitions(string providerName);
        IStructureMapResource StructureMaps(string providerName);
        ISubscriptionResource Subscriptions(string providerName);
        ISubstanceResource Substances(string providerName);
        ISupplyDeliveryResource SupplyDeliveries(string providerName);
        ISupplyRequestResource SupplyRequests(string providerName);
        ITaskResource Tasks(string providerName);
        ITestReportResource TestReports(string providerName);
        ITestScriptResource TestScripts(string providerName);
        IValueSetResource ValueSets(string providerName);
        IVisionPrescriptionResource VisionPrescriptions(string providerName);
    }
}
