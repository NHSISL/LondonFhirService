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
    "given": "nameGiven",
    "family": "nameFamily",
    "line": "addressLine",
    "identifier": "nhsNumber",
    "city": "addressCity",
    "district": "addressDistrict",
    "postalCode": "addressPostalCode",
    "country": "addressCountry"
};

// Every difference the comparison recorded against this field, on this side. The card highlights
// and their acceptance tick need the differences themselves rather than a single verdict: one
// field can carry more than one, and ticking has to write back to each.
export function getDiffsForField(
    diffs: DiffItemView[],
    field: string,
    side: ComparisonSide)
    : DiffItemView[] {
    return diffs.filter(diff => {
        if (getFieldFromPath(diff.path) !== field) {
            return false;
        }

        if (diff.type === "modified") {
            return true;
        }

        // A removal is something only the primary has; an addition, only the secondary.
        return side === "primary" ? diff.type === "removed" : diff.type === "added";
    });
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
