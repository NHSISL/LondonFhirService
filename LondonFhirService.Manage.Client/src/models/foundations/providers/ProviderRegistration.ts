// The add payload the API accepts. Audit fields are deliberately absent - the Manage host stamps
// CreatedBy, CreatedDate, UpdatedBy and UpdatedDate itself before it validates, so anything sent
// for them is overwritten.
export type ProviderRegistration = {
    id: string;
    friendlyName: string;
    fullyQualifiedName: string;
    fhirVersion: string;
    isActive: boolean;
    activeFrom: string | null;
    activeTo: string | null;
    isForComparisonOnly: boolean;
    isPrimary: boolean;
};
