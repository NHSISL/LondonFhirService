// The shape the add form holds while it is being edited. Dates are the raw datetime-local strings
// the inputs produce; the view service converts them on submit.
export type ProviderFormValues = {
    friendlyName: string;
    fullyQualifiedName: string;
    fhirVersion: string;
    isActive: boolean;
    activeFrom: string;
    activeTo: string;
    isPrimary: boolean;
    isForComparisonOnly: boolean;
};
