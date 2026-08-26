import type { CSSProperties } from "react";
import type { DiffItemView } from "../../models/views/comparisons/DiffItemView";

// Which side of the comparison a card is showing. The comparison engine compares the primary
// record as source1 and the secondary as source2, so a "removed" difference is something only the
// primary has and an "added" one is something only the secondary has.
export type ComparisonSide = "primary" | "secondary";

export type FieldDiffType = "added" | "removed" | "modified" | null;

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

export function getDiffTypeForField(
    diffs: DiffItemView[],
    field: string,
    side: ComparisonSide)
    : FieldDiffType {
    for (const diff of diffs) {
        if (getFieldFromPath(diff.path) !== field) {
            continue;
        }

        if (diff.type === "modified") {
            return "modified";
        }

        if (side === "primary" && diff.type === "removed") {
            return "removed";
        }

        if (side === "secondary" && diff.type === "added") {
            return "added";
        }
    }

    return null;
}

// An outline rather than a fill, so the highlight survives whatever the field itself renders -
// plain text, a badge, or a nested resource section.
export function getHighlightStyle(diffType: FieldDiffType): CSSProperties {
    if (diffType === null) {
        return {};
    }

    return {
        border: "2px solid #dc3545",
        borderRadius: "4px",
        padding: "8px",
        marginBottom: "8px"
    };
}

// A tighter outline for fields rendered inline inside an already indented panel, where the full
// padding above would push the row out of alignment with its neighbours.
export function getInlineHighlightStyle(hasDiff: boolean): CSSProperties {
    if (hasDiff === false) {
        return {};
    }

    return {
        border: "2px solid #dc3545",
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
