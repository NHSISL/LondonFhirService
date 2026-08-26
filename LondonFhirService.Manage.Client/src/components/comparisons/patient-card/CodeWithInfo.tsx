import { useState } from "react";
import { Button, Overlay, Popover } from "react-bootstrap";

type CodeWithInfoProps = {
    display: string | null;
    code: string | null;
    system: string | null;
};

// Two providers can send the same concept under different code systems, which is exactly the kind
// of difference that looks like a match until you check. The system is one hover away rather than
// inline, so the row stays readable.
export function CodeWithInfo({ display, code, system }: CodeWithInfoProps) {
    const [showSystem, setShowSystem] = useState<boolean>(false);
    const [infoButton, setInfoButton] = useState<HTMLButtonElement | null>(null);

    if (code === null && display === null) {
        return <span>N/A</span>;
    }

    return (
        <>
            <span className="d-inline-flex align-items-center gap-1">
                {display !== null && code !== null
                    ? <span>{display} ({code})</span>
                    : <span>{code ?? display}</span>}

                <Button
                    ref={setInfoButton}
                    variant="light"
                    size="sm"
                    className="p-0 border-0 bg-transparent"
                    style={{ fontSize: "0.75rem", lineHeight: 1, color: "#6c757d" }}
                    aria-label={`Show the code system for ${display ?? code}`}
                    onMouseEnter={() => setShowSystem(true)}
                    onMouseLeave={() => setShowSystem(false)}
                    onFocus={() => setShowSystem(true)}
                    onBlur={() => setShowSystem(false)}>
                    ⓘ
                </Button>
            </span>

            <Overlay show={showSystem} target={infoButton} placement="top" containerPadding={20}>
                <Popover>
                    <Popover.Body>
                        <small className="text-muted">System:</small>
                        <div className="font-monospace">{system ?? "Unknown system"}</div>
                    </Popover.Body>
                </Popover>
            </Overlay>
        </>
    );
}
