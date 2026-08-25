import { Table } from "react-bootstrap";
import { EmptyState } from "../shared/EmptyState";
import type { MetricSpanListProps } from "../../models/components/metrics/MetricSpanListProps";

const indentPerDepth = 16;

export function MetricSpanList({ spans }: MetricSpanListProps) {
    if (spans.length === 0) {
        return (
            <EmptyState
                title="No spans found"
                message="Nothing was recorded against this correlation id, or it has been purged." />
        );
    }

    return (
        <Table responsive size="sm" className="align-middle">
            <caption className="visually-hidden">
                Every span recorded against this correlation id, in the order the work started.
                Indentation shows which span each one ran inside.
            </caption>

            <thead>
                <tr>
                    <th scope="col">Span</th>
                    <th scope="col">Name</th>
                    <th scope="col">Target</th>
                    <th scope="col">Status</th>
                    <th scope="col">Started</th>
                    <th scope="col" className="text-end">Duration</th>
                    <th scope="col" className="text-end">Payload</th>
                    <th scope="col">Error</th>
                    <th scope="col">Description</th>
                </tr>
            </thead>

            <tbody>
                {spans.map(span => (
                    <tr key={span.id}>
                        <th
                            scope="row"
                            className="fw-normal text-nowrap"
                            style={{ paddingLeft: `${span.depth * indentPerDepth}px` }}>
                            {span.typeText}
                        </th>
                        <td className="text-break">{span.nameText}</td>
                        <td className="text-break">{span.targetText}</td>
                        <td>
                            <span className={span.statusClassName}>{span.statusText}</span>
                        </td>
                        <td className="text-nowrap">{span.startedText}</td>
                        <td className="text-end text-nowrap">{span.durationText}</td>
                        <td className="text-end text-nowrap">{span.payloadText}</td>
                        <td className="text-break">{span.errorCodeText}</td>
                        <td className="text-break">{span.descriptionText}</td>
                    </tr>
                ))}
            </tbody>
        </Table>
    );
}
