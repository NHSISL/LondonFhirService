import type { FhirResourceIndex } from "../../models/foundations/fhir/FhirResource";
import type { ListData } from "../../models/foundations/fhir/ListData";

// A provider's Lists only ever cover a subset of what it bundles - one feed might group
// Observations and Conditions into "Miscellaneous record" and "Problems" but never mention its
// Immunizations or Encounters in any List at all. Those resources still arrive as bare bundle
// entries, so a card that only walked List entries would never show them - not even as a raw
// reference - despite the comparison engine reporting differences against them.
//
// Returns every reference of the given resource type that no List's entry points at, in the
// bundle's own order, so a card can give resources like that a section of their own.
export function findUnlistedReferences(
    resourceIndex: FhirResourceIndex,
    lists: ListData[],
    resourceType: string)
    : string[] {
    const listedRefs = new Set(lists.flatMap(list => list.itemRefs));
    const prefix = `${resourceType}/`;

    return [...resourceIndex.keys()]
        .filter(reference => reference.startsWith(prefix) && listedRefs.has(reference) === false);
}

// Every reference of this resource type in the bundle, listed or not.
export function findAllReferences(resourceIndex: FhirResourceIndex, resourceType: string): string[] {
    const prefix = `${resourceType}/`;

    return [...resourceIndex.keys()].filter(reference => reference.startsWith(prefix));
}

// Resource types the card always groups under one section of its own, regardless of what a
// provider's List happens to reference - a feed can fold Observations into a List titled for
// something else entirely ("Miscellaneous record"), and grouping by that title would scatter one
// clinically distinct kind of fact under a name that says nothing about what it is. A List that
// only ever pointed at resources of these types therefore renders no items of its own; a mixed
// List keeps whatever else it has.
//
// Condition, AllergyIntolerance, MedicationStatement and Medication are deliberately left out -
// a provider's own list for those already reads as a dedicated section (typically "Problems",
// "Active Allergies", "Medication List"), so pulling them out again would just be churn.
export const dedicatedSectionResourceTypes = [
    "Observation",
    "Immunization",
    "Encounter",
    "FamilyMemberHistory",
    "MedicationRequest",
    "DiagnosticReport",
    "Procedure",
    "ProcedureRequest",
    "ReferralRequest",
    "Appointment",
    "Location",
    "RelatedPerson"
];
