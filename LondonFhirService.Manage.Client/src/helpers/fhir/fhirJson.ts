import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

// FHIR payloads reach this client as opaque JSON: the bundles come from third party providers, in
// whatever shape they chose to send, and the whole point of the comparison pages is that the two
// sides do not agree. So rather than assert a schema and fall over when a provider omits an
// element, every read goes through one of these and a missing or wrong typed value simply reads
// as absent.

export function readObject(value: unknown): FhirResource | null {
    return typeof value === "object" && value !== null && Array.isArray(value) === false
        ? value as FhirResource
        : null;
}

export function readArray(value: unknown): unknown[] {
    return Array.isArray(value) ? value : [];
}

export function readObjectArray(value: unknown): FhirResource[] {
    return readArray(value)
        .map(item => readObject(item))
        .filter((item): item is FhirResource => item !== null);
}

export function readString(value: unknown): string | null {
    return typeof value === "string" && value.length > 0 ? value : null;
}

export function readNumber(value: unknown): number | null {
    return typeof value === "number" && Number.isFinite(value) ? value : null;
}

export function readStringArray(value: unknown): string[] {
    return readArray(value)
        .map(item => readString(item))
        .filter((item): item is string => item !== null);
}

// Reads value at a path, stepping through objects only. `readPath(resource, "code", "text")`.
export function readPath(resource: FhirResource | null, ...path: string[]): unknown {
    let current: FhirResource | null = resource;

    for (let index = 0; index < path.length - 1; index++) {
        current = readObject(current?.[path[index]]);

        if (current === null) {
            return undefined;
        }
    }

    return current?.[path[path.length - 1]];
}

// The first coding of a CodeableConcept, which is where FHIR puts the primary code.
export function readFirstCoding(codeableConcept: unknown): FhirResource | null {
    return readObjectArray(readObject(codeableConcept)?.coding)[0] ?? null;
}

// clinicalStatus, verificationStatus and type are plain codes in STU3 but CodeableConcepts in R4.
// Both are read so a bundle from either version renders the same way.
export function readCode(value: unknown): string | null {
    const directValue = readString(value);

    if (directValue !== null) {
        return directValue;
    }

    const coding = readFirstCoding(value);

    return readString(coding?.display)
        ?? readString(coding?.code)
        ?? readString(readObject(value)?.text);
}

// An identifier whose system contains the given fragment, e.g. "nhs-number" or "sds-user-id".
export function findIdentifierBySystem(
    resource: FhirResource | null,
    systemFragment: string)
    : FhirResource | null {
    return readObjectArray(resource?.identifier)
        .find(identifier => readString(identifier.system)?.includes(systemFragment) === true)
        ?? null;
}
