// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Providers.FHIR.R4.Abstractions;
using LondonFhirService.Providers.FHIR.R4.Abstractions.Models.Resources;

namespace LondonFhirService.Core.Brokers.Fhirs
{
    public class FhirBroker : IFhirBroker
    {
        private readonly IFhirAbstractionProvider fhirAbstractionProvider;

        public FhirBroker(IFhirAbstractionProvider fhirAbstractionProvider)
        {
            this.fhirAbstractionProvider = fhirAbstractionProvider;
        }

        public IAccountResource Accounts(string providerName) =>
            fhirAbstractionProvider.Accounts(providerName);

        public IActivityDefinitionResource ActivityDefinitions(string providerName) =>
            fhirAbstractionProvider.ActivityDefinitions(providerName);

        public IAdverseEventResource AdverseEvents(string providerName) =>
            fhirAbstractionProvider.AdverseEvents(providerName);

        public IAllergyIntoleranceResource AllergyIntolerances(string providerName) =>
            fhirAbstractionProvider.AllergyIntolerances(providerName);

        public IAppointmentResponseResource AppointmentResponses(string providerName) =>
            fhirAbstractionProvider.AppointmentResponses(providerName);

        public IAppointmentResource Appointments(string providerName) =>
            fhirAbstractionProvider.Appointments(providerName);

        public IAuditEventResource AuditEvents(string providerName) =>
            fhirAbstractionProvider.AuditEvents(providerName);

        public IBasicResource Basics(string providerName) =>
            fhirAbstractionProvider.Basics(providerName);

        public IBinaryResource Binaries(string providerName) =>
            fhirAbstractionProvider.Binaries(providerName);

        public IBiologicallyDerivedProductResource BiologicallyDerivedProducts(string providerName) =>
            fhirAbstractionProvider.BiologicallyDerivedProducts(providerName);

        public IBodyStructureResource BodyStructures(string providerName) =>
            fhirAbstractionProvider.BodyStructures(providerName);

        public IBundleResource Bundles(string providerName) =>
            fhirAbstractionProvider.Bundles(providerName);

        public ICapabilityStatementResource CapabilityStatements(string providerName) =>
            fhirAbstractionProvider.CapabilityStatements(providerName);

        public ICarePlanResource CarePlans(string providerName) =>
            fhirAbstractionProvider.CarePlans(providerName);

        public ICareTeamResource CareTeams(string providerName) =>
            fhirAbstractionProvider.CareTeams(providerName);

        public ICatalogEntryResource CatalogEntries(string providerName) =>
            fhirAbstractionProvider.CatalogEntries(providerName);

        public IChargeItemDefinitionResource ChargeItemDefinitions(string providerName) =>
            fhirAbstractionProvider.ChargeItemDefinitions(providerName);

        public IChargeItemResource ChargeItems(string providerName) =>
            fhirAbstractionProvider.ChargeItems(providerName);

        public IClaimResponseResource ClaimResponses(string providerName) =>
            fhirAbstractionProvider.ClaimResponses(providerName);

        public IClaimResource Claims(string providerName) =>
            fhirAbstractionProvider.Claims(providerName);

        public IClinicalImpressionResource ClinicalImpressions(string providerName) =>
            fhirAbstractionProvider.ClinicalImpressions(providerName);

        public ICodeSystemResource CodeSystems(string providerName) =>
            fhirAbstractionProvider.CodeSystems(providerName);

        public ICommunicationRequestResource CommunicationRequests(string providerName) =>
            fhirAbstractionProvider.CommunicationRequests(providerName);

        public ICommunicationResource Communications(string providerName) =>
            fhirAbstractionProvider.Communications(providerName);

        public ICompartmentDefinitionResource CompartmentDefinitions(string providerName) =>
            fhirAbstractionProvider.CompartmentDefinitions(providerName);

        public ICompositionResource Compositions(string providerName) =>
            fhirAbstractionProvider.Compositions(providerName);

        public IConceptMapResource ConceptMaps(string providerName) =>
            fhirAbstractionProvider.ConceptMaps(providerName);

        public IConditionResource Conditions(string providerName) =>
            fhirAbstractionProvider.Conditions(providerName);

        public IConsentResource Consents(string providerName) =>
            fhirAbstractionProvider.Consents(providerName);

        public IContractResource Contracts(string providerName) =>
            fhirAbstractionProvider.Contracts(providerName);

        public ICoverageEligibilityRequestResource CoverageEligibilityRequests(string providerName) =>
            fhirAbstractionProvider.CoverageEligibilityRequests(providerName);

        public ICoverageEligibilityResponseResource CoverageEligibilityResponses(string providerName) =>
            fhirAbstractionProvider.CoverageEligibilityResponses(providerName);

        public ICoverageResource Coverages(string providerName) =>
            fhirAbstractionProvider.Coverages(providerName);

        public IDetectedIssueResource DetectedIssues(string providerName) =>
            fhirAbstractionProvider.DetectedIssues(providerName);

        public IDeviceDefinitionResource DeviceDefinitions(string providerName) =>
            fhirAbstractionProvider.DeviceDefinitions(providerName);

        public IDeviceMetricResource DeviceMetrics(string providerName) =>
            fhirAbstractionProvider.DeviceMetrics(providerName);

        public IDeviceRequestResource DeviceRequests(string providerName) =>
            fhirAbstractionProvider.DeviceRequests(providerName);

        public IDeviceResource Devices(string providerName) =>
            fhirAbstractionProvider.Devices(providerName);

        public IDeviceUseStatementResource DeviceUseStatements(string providerName) =>
            fhirAbstractionProvider.DeviceUseStatements(providerName);

        public IDiagnosticReportResource DiagnosticReports(string providerName) =>
            fhirAbstractionProvider.DiagnosticReports(providerName);

        public IDocumentManifestResource DocumentManifests(string providerName) =>
            fhirAbstractionProvider.DocumentManifests(providerName);

        public IDocumentReferenceResource DocumentReferences(string providerName) =>
            fhirAbstractionProvider.DocumentReferences(providerName);

        public IEffectEvidenceSynthesisResource EffectEvidenceSyntheses(string providerName) =>
            fhirAbstractionProvider.EffectEvidenceSyntheses(providerName);

        public IEncounterResource Encounters(string providerName) =>
            fhirAbstractionProvider.Encounters(providerName);

        public IEndpointResource Endpoints(string providerName) =>
            fhirAbstractionProvider.Endpoints(providerName);

        public IEnrollmentRequestResource EnrollmentRequests(string providerName) =>
            fhirAbstractionProvider.EnrollmentRequests(providerName);

        public IEnrollmentResponseResource EnrollmentResponses(string providerName) =>
            fhirAbstractionProvider.EnrollmentResponses(providerName);

        public IEpisodeOfCareResource EpisodesOfCare(string providerName) =>
            fhirAbstractionProvider.EpisodesOfCare(providerName);

        public IEventDefinitionResource EventDefinitions(string providerName) =>
            fhirAbstractionProvider.EventDefinitions(providerName);

        public IEvidenceResource Evidences(string providerName) =>
            fhirAbstractionProvider.Evidences(providerName);

        public IEvidenceVariableResource EvidenceVariables(string providerName) =>
            fhirAbstractionProvider.EvidenceVariables(providerName);

        public IExampleScenarioResource ExampleScenarios(string providerName) =>
            fhirAbstractionProvider.ExampleScenarios(providerName);

        public IExplanationOfBenefitResource ExplanationsOfBenefit(string providerName) =>
            fhirAbstractionProvider.ExplanationsOfBenefit(providerName);

        public IFamilyMemberHistoryResource FamilyMemberHistories(string providerName) =>
            fhirAbstractionProvider.FamilyMemberHistories(providerName);

        public IFlagResource Flags(string providerName) =>
            fhirAbstractionProvider.Flags(providerName);

        public IGoalResource Goals(string providerName) =>
            fhirAbstractionProvider.Goals(providerName);

        public IGraphDefinitionResource GraphDefinitions(string providerName) =>
            fhirAbstractionProvider.GraphDefinitions(providerName);

        public IGroupResource Groups(string providerName) =>
            fhirAbstractionProvider.Groups(providerName);

        public IGuidanceResponseResource GuidanceResponses(string providerName) =>
            fhirAbstractionProvider.GuidanceResponses(providerName);

        public IHealthcareServiceResource HealthcareServices(string providerName) =>
            fhirAbstractionProvider.HealthcareServices(providerName);

        public IImagingStudyResource ImagingStudies(string providerName) =>
            fhirAbstractionProvider.ImagingStudies(providerName);

        public IImmunizationEvaluationResource ImmunizationEvaluations(string providerName) =>
            fhirAbstractionProvider.ImmunizationEvaluations(providerName);

        public IImmunizationRecommendationResource ImmunizationRecommendations(string providerName) =>
            fhirAbstractionProvider.ImmunizationRecommendations(providerName);

        public IImmunizationResource Immunizations(string providerName) =>
            fhirAbstractionProvider.Immunizations(providerName);

        public IImplementationGuideResource ImplementationGuides(string providerName) =>
            fhirAbstractionProvider.ImplementationGuides(providerName);

        public IInsurancePlanResource InsurancePlans(string providerName) =>
            fhirAbstractionProvider.InsurancePlans(providerName);

        public IInvoiceResource Invoices(string providerName) =>
            fhirAbstractionProvider.Invoices(providerName);

        public ILibraryResource Libraries(string providerName) =>
            fhirAbstractionProvider.Libraries(providerName);

        public ILinkageResource Linkages(string providerName) =>
            fhirAbstractionProvider.Linkages(providerName);

        public IListResource Lists(string providerName) =>
            fhirAbstractionProvider.Lists(providerName);

        public ILocationResource Locations(string providerName) =>
            fhirAbstractionProvider.Locations(providerName);

        public IMeasureReportResource MeasureReports(string providerName) =>
            fhirAbstractionProvider.MeasureReports(providerName);

        public IMeasureResource Measures(string providerName) =>
            fhirAbstractionProvider.Measures(providerName);

        public IMediaResource Media(string providerName) =>
            fhirAbstractionProvider.Media(providerName);

        public IMedicationAdministrationResource MedicationAdministrations(string providerName) =>
            fhirAbstractionProvider.MedicationAdministrations(providerName);

        public IMedicationDispenseResource MedicationDispenses(string providerName) =>
            fhirAbstractionProvider.MedicationDispenses(providerName);

        public IMedicationKnowledgeResource MedicationKnowledge(string providerName) =>
            fhirAbstractionProvider.MedicationKnowledge(providerName);

        public IMedicationRequestResource MedicationRequests(string providerName) =>
            fhirAbstractionProvider.MedicationRequests(providerName);

        public IMedicationResource Medications(string providerName) =>
            fhirAbstractionProvider.Medications(providerName);

        public IMedicationStatementResource MedicationStatements(string providerName) =>
            fhirAbstractionProvider.MedicationStatements(providerName);

        public IMedicinalProductAuthorizationResource MedicinalProductAuthorizations(string providerName) =>
            fhirAbstractionProvider.MedicinalProductAuthorizations(providerName);

        public IMedicinalProductContraindicationResource MedicinalProductContraindications(string providerName) =>
            fhirAbstractionProvider.MedicinalProductContraindications(providerName);

        public IMedicinalProductIndicationResource MedicinalProductIndications(string providerName) =>
            fhirAbstractionProvider.MedicinalProductIndications(providerName);

        public IMedicinalProductIngredientResource MedicinalProductIngredients(string providerName) =>
            fhirAbstractionProvider.MedicinalProductIngredients(providerName);

        public IMedicinalProductInteractionResource MedicinalProductInteractions(string providerName) =>
            fhirAbstractionProvider.MedicinalProductInteractions(providerName);

        public IMedicinalProductManufacturedResource MedicinalProductManufactureds(string providerName) =>
            fhirAbstractionProvider.MedicinalProductManufactureds(providerName);

        public IMedicinalProductPackagedResource MedicinalProductPackageds(string providerName) =>
            fhirAbstractionProvider.MedicinalProductPackageds(providerName);

        public IMedicinalProductPharmaceuticalResource MedicinalProductPharmaceuticals(string providerName) =>
            fhirAbstractionProvider.MedicinalProductPharmaceuticals(providerName);

        public IMedicinalProductResource MedicinalProducts(string providerName) =>
            fhirAbstractionProvider.MedicinalProducts(providerName);

        public IMedicinalProductUndesirableEffectResource MedicinalProductUndesirableEffects(string providerName) =>
            fhirAbstractionProvider.MedicinalProductUndesirableEffects(providerName);

        public IMessageDefinitionResource MessageDefinitions(string providerName) =>
            fhirAbstractionProvider.MessageDefinitions(providerName);

        public IMessageHeaderResource MessageHeaders(string providerName) =>
            fhirAbstractionProvider.MessageHeaders(providerName);

        public IMolecularSequenceResource MolecularSequences(string providerName) =>
            fhirAbstractionProvider.MolecularSequences(providerName);

        public INamingSystemResource NamingSystems(string providerName) =>
            fhirAbstractionProvider.NamingSystems(providerName);

        public INutritionOrderResource NutritionOrders(string providerName) =>
            fhirAbstractionProvider.NutritionOrders(providerName);

        public IObservationDefinitionResource ObservationDefinitions(string providerName) =>
            fhirAbstractionProvider.ObservationDefinitions(providerName);

        public IObservationResource Observations(string providerName) =>
            fhirAbstractionProvider.Observations(providerName);

        public IOperationDefinitionResource OperationDefinitions(string providerName) =>
            fhirAbstractionProvider.OperationDefinitions(providerName);

        public IOperationOutcomeResource OperationOutcomes(string providerName) =>
            fhirAbstractionProvider.OperationOutcomes(providerName);

        public IOrganizationAffiliationResource OrganizationAffiliations(string providerName) =>
            fhirAbstractionProvider.OrganizationAffiliations(providerName);

        public IOrganizationResource Organizations(string providerName) =>
            fhirAbstractionProvider.Organizations(providerName);

        public IParametersResource Parameters(string providerName) =>
            fhirAbstractionProvider.Parameters(providerName);

        public IPatientResource Patients(string providerName) =>
            fhirAbstractionProvider.Patients(providerName);

        public IPaymentNoticeResource PaymentNotices(string providerName) =>
            fhirAbstractionProvider.PaymentNotices(providerName);

        public IPaymentReconciliationResource PaymentReconciliations(string providerName) =>
            fhirAbstractionProvider.PaymentReconciliations(providerName);

        public IPersonResource Persons(string providerName) =>
            fhirAbstractionProvider.Persons(providerName);

        public IPlanDefinitionResource PlanDefinitions(string providerName) =>
            fhirAbstractionProvider.PlanDefinitions(providerName);

        public IPractitionerRoleResource PractitionerRoles(string providerName) =>
            fhirAbstractionProvider.PractitionerRoles(providerName);

        public IPractitionerResource Practitioners(string providerName) =>
            fhirAbstractionProvider.Practitioners(providerName);

        public IProcedureResource Procedures(string providerName) =>
            fhirAbstractionProvider.Procedures(providerName);

        public IProvenanceResource Provenances(string providerName) =>
            fhirAbstractionProvider.Provenances(providerName);

        public IQuestionnaireResponseResource QuestionnaireResponses(string providerName) =>
            fhirAbstractionProvider.QuestionnaireResponses(providerName);

        public IQuestionnaireResource Questionnaires(string providerName) =>
            fhirAbstractionProvider.Questionnaires(providerName);

        public IRelatedPersonResource RelatedPersons(string providerName) =>
            fhirAbstractionProvider.RelatedPersons(providerName);

        public IRequestGroupResource RequestGroups(string providerName) =>
            fhirAbstractionProvider.RequestGroups(providerName);

        public IResearchDefinitionResource ResearchDefinitions(string providerName) =>
            fhirAbstractionProvider.ResearchDefinitions(providerName);

        public IResearchElementDefinitionResource ResearchElementDefinitions(string providerName) =>
            fhirAbstractionProvider.ResearchElementDefinitions(providerName);

        public IResearchStudyResource ResearchStudies(string providerName) =>
            fhirAbstractionProvider.ResearchStudies(providerName);

        public IResearchSubjectResource ResearchSubjects(string providerName) =>
            fhirAbstractionProvider.ResearchSubjects(providerName);

        public IRiskAssessmentResource RiskAssessments(string providerName) =>
            fhirAbstractionProvider.RiskAssessments(providerName);

        public IRiskEvidenceSynthesisResource RiskEvidenceSyntheses(string providerName) =>
            fhirAbstractionProvider.RiskEvidenceSyntheses(providerName);

        public IScheduleResource Schedules(string providerName) =>
            fhirAbstractionProvider.Schedules(providerName);

        public ISearchParameterResource SearchParameters(string providerName) =>
            fhirAbstractionProvider.SearchParameters(providerName);

        public IServiceRequestResource ServiceRequests(string providerName) =>
            fhirAbstractionProvider.ServiceRequests(providerName);

        public ISlotResource Slots(string providerName) =>
            fhirAbstractionProvider.Slots(providerName);

        public ISpecimenDefinitionResource SpecimenDefinitions(string providerName) =>
            fhirAbstractionProvider.SpecimenDefinitions(providerName);

        public ISpecimenResource Specimens(string providerName) =>
            fhirAbstractionProvider.Specimens(providerName);

        public IStructureDefinitionResource StructureDefinitions(string providerName) =>
            fhirAbstractionProvider.StructureDefinitions(providerName);

        public IStructureMapResource StructureMaps(string providerName) =>
            fhirAbstractionProvider.StructureMaps(providerName);

        public ISubscriptionResource Subscriptions(string providerName) =>
            fhirAbstractionProvider.Subscriptions(providerName);

        public ISubstanceNucleicAcidResource SubstanceNucleicAcids(string providerName) =>
            fhirAbstractionProvider.SubstanceNucleicAcids(providerName);

        public ISubstancePolymerResource SubstancePolymers(string providerName) =>
            fhirAbstractionProvider.SubstancePolymers(providerName);

        public ISubstanceProteinResource SubstanceProteins(string providerName) =>
            fhirAbstractionProvider.SubstanceProteins(providerName);

        public ISubstanceReferenceInformationResource SubstanceReferenceInformations(string providerName) =>
            fhirAbstractionProvider.SubstanceReferenceInformations(providerName);

        public ISubstanceResource Substances(string providerName) =>
            fhirAbstractionProvider.Substances(providerName);

        public ISubstanceSourceMaterialResource SubstanceSourceMaterials(string providerName) =>
            fhirAbstractionProvider.SubstanceSourceMaterials(providerName);

        public ISubstanceSpecificationResource SubstanceSpecifications(string providerName) =>
            fhirAbstractionProvider.SubstanceSpecifications(providerName);

        public ISupplyDeliveryResource SupplyDeliveries(string providerName) =>
            fhirAbstractionProvider.SupplyDeliveries(providerName);

        public ISupplyRequestResource SupplyRequests(string providerName) =>
            fhirAbstractionProvider.SupplyRequests(providerName);

        public ITaskResource Tasks(string providerName) =>
            fhirAbstractionProvider.Tasks(providerName);

        public ITerminologyCapabilitiesResource TerminologyCapabilities(string providerName) =>
            fhirAbstractionProvider.TerminologyCapabilities(providerName);

        public ITestReportResource TestReports(string providerName) =>
            fhirAbstractionProvider.TestReports(providerName);

        public ITestScriptResource TestScripts(string providerName) =>
            fhirAbstractionProvider.TestScripts(providerName);

        public IValueSetResource ValueSets(string providerName) =>
            fhirAbstractionProvider.ValueSets(providerName);

        public IVerificationResultResource VerificationResults(string providerName) =>
            fhirAbstractionProvider.VerificationResults(providerName);

        public IVisionPrescriptionResource VisionPrescriptions(string providerName) =>
            fhirAbstractionProvider.VisionPrescriptions(providerName);
    }
}
