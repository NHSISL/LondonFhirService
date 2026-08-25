// The field keyed error bag the API returns on a 400, as processApiErrors expects it.
export type ProviderFormApiErrors = {
    friendlyName?: string[];
    fullyQualifiedName?: string[];
    fhirVersion?: string[];
};
