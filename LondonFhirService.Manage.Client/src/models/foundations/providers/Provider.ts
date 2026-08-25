// Foundation model - mirrors LondonFhirService.Core.Models.Foundations.Providers.Provider
// as it is serialised by the /api/providers endpoint. No UI concerns live here.
export type Provider = {
    id: string;
    friendlyName: string;
    fullyQualifiedName: string;
    fhirVersion: string;
    isActive: boolean;
    activeFrom: string | null;
    activeTo: string | null;
    isForComparisonOnly: boolean;
    isPrimary: boolean;
    createdBy: string;
    createdDate: string;
    updatedBy: string;
    updatedDate: string;
};
