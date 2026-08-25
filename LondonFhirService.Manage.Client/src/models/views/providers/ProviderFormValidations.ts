import type { Validation } from "../../validations/validation";

// Lengths mirror the ones ValidateProviderOnAdd enforces on the server, so the form catches them
// before the round trip rather than after.
export const providerFormValidations: Validation[] = [
    {
        property: "friendlyName",
        friendlyName: "Friendly name",
        isRequired: true,
        maxLength: 255
    },
    {
        property: "fullyQualifiedName",
        friendlyName: "Fully qualified name",
        isRequired: true,
        maxLength: 500
    },
    {
        property: "fhirVersion",
        friendlyName: "FHIR version",
        isRequired: true,
        maxLength: 10
    }
];
