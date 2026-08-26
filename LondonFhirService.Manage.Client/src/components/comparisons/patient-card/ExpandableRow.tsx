import type { ReactNode } from "react";

type ExpandableRowProps = {
    expanded: boolean;
    onToggle: () => void;

    // Goes inside the toggle, so it must stay non-interactive - a control nested in a button is
    // invalid markup and would be unreachable by keyboard.
    label: ReactNode;

    // Sits beside the toggle, for the parts of a row that are themselves interactive: a code's
    // system popover, mainly.
    trailing?: ReactNode;

    badges?: ReactNode;
};

// The disclosure row every resource in the tree shares. The caret and label are one real button,
// so the tree can be walked with a keyboard as well as a mouse - the POC's rows were divs and
// could only be opened by clicking them.
export function ExpandableRow({ expanded, onToggle, label, trailing, badges }: ExpandableRowProps) {
    return (
        <div className="d-flex align-items-center gap-2 flex-wrap">
            <button
                type="button"
                className={"d-flex align-items-center gap-2 text-start border-0 bg-transparent "
                    + "p-0 text-body"}
                style={{ userSelect: "none" }}
                onClick={onToggle}
                aria-expanded={expanded}>
                <span className="small" aria-hidden="true">{expanded ? "▼" : "▶"}</span>
                {label}
            </button>

            {trailing}
            {badges}
        </div>
    );
}
