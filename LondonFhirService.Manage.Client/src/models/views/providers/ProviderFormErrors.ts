import type { ErrorBase } from "../../../types/ErrorBase";

export type ProviderFormErrors = ErrorBase & {
    friendlyName: string;
    fullyQualifiedName: string;
    fhirVersion: string;
};
