import type { CSSProperties } from "react";
import type { DiffItemView } from "../../models/views/comparisons/DiffItemView";

// Which side of the comparison a card is showing. The comparison engine compares the primary
// record as source1 and the secondary as source2, so a "removed" difference is something only the
// primary has and an "added" one is something only the secondary has.
export type ComparisonSide = "primary" | "secondary";

// Bootstrap's danger and success, hard coded because these are inline styles on arbitrary content
// rather than classes on a component.
const outstandingColour = "#dc3545";
const acceptedColour = "#198754";

// A difference's path names the JSON element that changed, not the field the patient card renders.
// This maps the tail of a path back onto a card field so the right box gets outlined.
//
// Every value here must appear in patientHighlightFields, and the card must draw a box for it.
// A field that maps but is never rendered would drop its differences out of the card silently
// while the differences list still counted them - diffHighlighting.test.ts enforces the first
// half of that.
const pathPartToField: Record<string, string> = {
    "birthDate": "birthDate",
    "gender": "gender",
    "name": "nameFamily",
    "namePrefix": "namePrefix",
    "nameSuffix": "nameSuffix",
    "addressLine": "addressLine",
    "addressCity": "addressCity",
    "addressDistrict": "addressDistrict",
    "addressPostalCode": "addressPostalCode",
    "addressCountry": "addressCountry",
    "telecom": "telecom",
    "communication": "communication",
    "managingOrganization": "managingOrganizationRef",
    "generalPractitioner": "generalPractitionerRefs",
    "given": "nameGiven",
    "family": "nameFamily",
    "line": "addressLine",
    "identifier": "nhsNumber",
    "city": "addressCity",
    "district": "addressDistrict",
    "postalCode": "addressPostalCode",
    "country": "addressCountry"
};

// The card fields that get a highlight box. Kept beside the map because the two have to agree:
// a difference whose path maps to a field nothing renders would vanish from the card while still
// being counted in the differences list, which is how a generalPractitioner change went missing
// before this list existed.
export const patientHighlightFields = [
    "nhsNumber",
    "nameFamily",
    "nameGiven",
    "namePrefix",
    "nameSuffix",
    "birthDate",
    "gender",
    "addressLine",
    "addressCity",
    "addressDistrict",
    "addressPostalCode",
    "addressCountry",
    "telecom",
    "communication",
    "managingOrganizationRef",
    "generalPractitionerRefs"
];

// Every difference the comparison recorded against this field, on this side. The card highlights
// and their acceptance tick need the differences themselves rather than a single verdict: one
// field can carry more than one, and ticking has to write back to each.
export function getDiffsForField(
    diffs: DiffItemView[],
    field: string,
    side: ComparisonSide)
    : DiffItemView[] {
    return getDiffsForFields(diffs, [field], side);
}

// One box can stand for several fields - the header renders the whole formatted name, so a change
// to any part of it belongs in the same outline.
export function getDiffsForFields(
    diffs: DiffItemView[],
    fields: string[],
    side: ComparisonSide)
    : DiffItemView[] {
    return diffs.filter(diff => {
        const field = getFieldFromPath(diff.path);

        return field !== null && fields.includes(field) && appliesToSide(diff, side);
    });
}

// The Patient differences no field on the card claims: a change somewhere in the resource the
// card does not lay out, such as an extension or a marital status. They are listed on their own
// rather than dropped, so what the card shows adds up to what the differences list counts.
export function getUnhighlightedPatientDiffs(
    diffs: DiffItemView[],
    side: ComparisonSide)
    : DiffItemView[] {
    return diffs.filter(diff =>
        getFieldFromPath(diff.path) === null && appliesToSide(diff, side));
}

// The resource types the card lays out a section for. Everything else the comparison can
// report - a RelatedPerson, which has no matcher registered server side at all - has nowhere of
// its own to appear.
const sectionedResourceTypes = [
    "Patient",
    "EpisodeOfCare",
    "List",
    "MedicationStatement",
    "Observation",
    "Condition",
    "AllergyIntolerance",
    "Immunization",
    "Encounter",
    "FamilyMemberHistory",
    "PractitionerRole",
    "Organization",
    "Practitioner",
    "Location",
    "MedicationRequest",
    "DiagnosticReport",
    "Procedure",
    "ProcedureRequest",
    "ReferralRequest",
    "Appointment"
];

// Resource types whose own item rows show a manual-review-required diff themselves, attached to
// every resource that shares the key the engine could not resolve - see getAmbiguousDiffs. A
// manual-review-required diff for one of these never needs to fall through to Other differences;
// it already has an honest home. Extend this list if another sectioned type grows the same
// duplicate-identifier ambiguity in its own data.
const ambiguousDiffItemResourceTypes = ["Observation", "FamilyMemberHistory"];

// Every difference the card would otherwise not show: the Patient ones no field claims, and all
// of those against a resource type the card has no section for. Without this the card and the
// differences list disagree on how many differences there are, and the ones missing from the card
// cannot be ticked as acceptable from it.
export function getOtherDiffs(diffs: DiffItemView[], side: ComparisonSide): DiffItemView[] {
    return diffs.filter(diff => {
        if (appliesToSide(diff, side) === false) {
            return false;
        }

        // No matched resource means no item to attach the diff to, whatever its resource type -
        // this is the only place left for it to be seen and accepted. Except the resource types
        // that show it on their own item rows instead - see ambiguousDiffItemResourceTypes.
        if (diff.type === "manual-review-required") {
            return ambiguousDiffItemResourceTypes.includes(diff.resourceTypeText ?? "") === false;
        }

        const resourceType = diff.resourceTypeText ?? "";

        if (resourceType === "Patient") {
            return getFieldFromPath(diff.path) === null;
        }

        return sectionedResourceTypes.includes(resourceType) === false;
    });
}

// A removal is something only the primary has; an addition, only the secondary. A modification is
// on both. A manual-review-required diff has no matched counterpart to say which side it belongs
// to, so it is shown on both - the alternative is hiding it everywhere.
function appliesToSide(diff: DiffItemView, side: ComparisonSide): boolean {
    if (diff.type === "modified" || diff.type === "manual-review-required") {
        return true;
    }

    return side === "primary" ? diff.type === "removed" : diff.type === "added";
}

// Nothing to show, something still outstanding, or everything here already accepted. Acceptance is
// a property of the difference rather than of a side, so a field reads the same on both cards -
// which is the point: an accepted difference should stop drawing the eye on either.
export type FieldDiffState = "none" | "outstanding" | "accepted";

export function getDiffState(fieldDiffs: DiffItemView[]): FieldDiffState {
    if (fieldDiffs.length === 0) {
        return "none";
    }

    return fieldDiffs.every(diff => diff.acceptableDiff) ? "accepted" : "outstanding";
}

// An outline rather than a fill, so the highlight survives whatever the field itself renders -
// plain text, a badge, or a nested resource section.
export function getHighlightStyle(state: FieldDiffState): CSSProperties {
    if (state === "none") {
        return {};
    }

    return {
        border: `2px solid ${state === "accepted" ? acceptedColour : outstandingColour}`,
        borderRadius: "4px",
        padding: "8px",
        marginBottom: "8px"
    };
}

// A tighter outline for fields rendered inline inside an already indented panel, where the full
// padding above would push the row out of alignment with its neighbours.
export function getInlineHighlightStyle(state: FieldDiffState): CSSProperties {
    if (state === "none") {
        return {};
    }

    return {
        border: `2px solid ${state === "accepted" ? acceptedColour : outstandingColour}`,
        borderRadius: "4px",
        padding: "2px 4px",
        margin: "-2px -4px"
    };
}

// The distinct fields a path can map to. Exported for the test that holds it to
// patientHighlightFields.
export function getMappableFields(): string[] {
    return [...new Set(Object.values(pathPartToField))];
}

// Paths look like `name[0]."family"`, so each part is stripped of its index and quoting before it
// is looked up. The tail is read first: the last recognised part is the most specific.
function getFieldFromPath(path: string): string | null {
    const parts = path.split(".");

    for (let index = parts.length - 1; index >= 0; index--) {
        const part = parts[index].replace(/\[\d+\]/g, "").replace(/"+/g, "");

        if (pathPartToField[part]) {
            return pathPartToField[part];
        }
    }

    return null;
}
