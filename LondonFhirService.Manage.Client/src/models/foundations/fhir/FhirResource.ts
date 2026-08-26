// A resource as it arrives inside a bundle. FHIR payloads are read here rather than modelled -
// two providers can send the same clinical fact with different optional elements present, and a
// viewer that throws away what it does not recognise would hide exactly the differences these
// pages exist to show. Every read goes through helpers/fhir/fhirJson so an unexpected shape
// degrades to a missing value instead of a crash.
export type FhirResource = Record<string, unknown>;

// Keyed by "ResourceType/id", the form a Reference resolves to once the base URL is stripped.
export type FhirResourceIndex = Map<string, FhirResource>;
