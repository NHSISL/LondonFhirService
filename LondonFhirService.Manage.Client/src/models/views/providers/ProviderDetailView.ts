// UI-ready projection produced by the provider view service for the detail page.
import type { ProviderFormValues } from "./ProviderFormValues";

export type ProviderDetailView = {
    id: string;
    friendlyName: string;
    fullyQualifiedName: string;
    fhirVersionText: string;
    statusText: string;
    statusClassName: string;
    roleText: string;
    roleClassName: string;
    isPrimaryText: string;
    isForComparisonOnlyText: string;
    activeFromText: string;
    activeToText: string;
    activePeriodText: string;
    createdBy: string;
    createdDateText: string;
    updatedBy: string;
    updatedDateText: string;
    detailUrl: string;

    // The same record in the shape the edit form binds to, so opening edit does not need a
    // second read or a parse of the already formatted display strings.
    editValues: ProviderFormValues;
};
