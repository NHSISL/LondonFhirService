import { Card } from "react-bootstrap";
import type { MetricTimelineProps } from "../../models/components/metrics/MetricTimelineProps";

const indentPerDepth = 12;

export function MetricTimeline({ correlation }: MetricTimelineProps) {
    if (correlation.spans.length === 0) {
        return <></>;
    }

    return (
        <Card className="mb-3">
            <Card.Header className="d-flex flex-wrap align-items-center gap-2">
                <h2 className="h6 mb-0 me-auto">Timeline</h2>
                <span className="text-muted small">
                    {correlation.windowText} from first start to last finish
                </span>
            </Card.Header>

            <Card.Body>
                {/*
                    A list rather than a table: each row is one bar, and the accompanying figures
                    are already in the span table below, so the bars carry a text alternative
                    instead of repeating every column.
                */}
                <ul className="list-unstyled mb-0">
                    {correlation.spans.map(span => (
                        <li key={span.id} className="d-flex align-items-center gap-2 mb-1">
                            <span
                                className="text-nowrap small text-truncate"
                                style={{
                                    width: "190px",
                                    paddingLeft: `${span.depth * indentPerDepth}px`
                                }}
                                title={span.labelText}>
                                {span.labelText}
                            </span>

                            <span
                                className="position-relative flex-grow-1 rounded"
                                style={{ height: "14px", backgroundColor: "rgba(0,0,0,.05)" }}
                                title={span.labelText}>
                                <span
                                    className={`position-absolute top-0 rounded ${span.barClassName}`}
                                    style={{
                                        left: `${span.offsetPercent}%`,
                                        width: `${span.widthPercent}%`,
                                        height: "14px"
                                    }}
                                    title={span.labelText} />

                                <span className="visually-hidden">
                                    {span.labelText} {span.nameText}, {span.statusText}, starts
                                    {" "}{span.offsetText} into the request and takes
                                    {" "}{span.durationText}.
                                </span>
                            </span>

                            <span
                                className="text-nowrap small text-muted text-end"
                                style={{ width: "80px" }}>
                                {span.durationText}
                            </span>
                        </li>
                    ))}
                </ul>
            </Card.Body>
        </Card>
    );
}
