import type { PrimarySourceBadgeProps } from "../../models/components/comparisons/PrimarySourceBadgeProps";

// One chip, rendered in the two tables that stack on the comparisons screen. Hand-written twice,
// the two could be restyled or relabelled apart - which on a page that shows both at once would
// read as two different facts rather than one.
export function PrimarySourceBadge({ isPrimarySource }: PrimarySourceBadgeProps) {
    if (isPrimarySource === false) {
        return null;
    }

    return (
        <>
            {" "}
            <span className="badge bg-primary" title="This source is the primary">
                Primary
            </span>
        </>
    );
}
