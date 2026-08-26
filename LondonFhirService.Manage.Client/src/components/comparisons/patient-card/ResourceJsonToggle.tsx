import { useState } from "react";
import { Button } from "react-bootstrap";
import type { FhirResource } from "../../../models/foundations/fhir/FhirResource";

type ResourceJsonToggleProps = {
    resource: FhirResource | null;
    className?: string;
};

// Every resource in the tree can be opened to its raw JSON, because the parsed view deliberately
// shows only the elements the comparison cares about and an operator chasing an unexplained
// difference needs the rest.
export function ResourceJsonToggle({ resource, className }: ResourceJsonToggleProps) {
    const [showJson, setShowJson] = useState<boolean>(false);

    if (resource === null) {
        return null;
    }

    return (
        <>
            <Button
                variant="link"
                size="sm"
                className={className ?? "p-0 mt-2"}
                onClick={() => setShowJson(currentValue => currentValue === false)}>
                {showJson ? "Hide JSON" : "Show JSON"}
            </Button>

            {showJson && (
                <pre
                    className="bg-white p-2 mt-2 small border rounded"
                    style={{ maxHeight: "200px", overflow: "auto" }}>
                    {JSON.stringify(resource, null, 2)}
                </pre>
            )}
        </>
    );
}
