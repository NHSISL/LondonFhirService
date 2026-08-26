import { Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { EmptyState } from "../shared/EmptyState";
import type { ComparisonListProps } from "../../models/components/comparisons/ComparisonListProps";

export function ComparisonList({ comparisons, selectedComparisonId }: ComparisonListProps) {
    if (comparisons.length === 0) {
        return (
            <EmptyState
                title="No comparisons found"
                message={"No comparison matches your search. Try a different correlation id or "
                    + "comment, or clear the unresolved filter."} />
        );
    }

    return (
        <Table hover responsive className="align-middle">
            <caption className="visually-hidden">
                Comparisons between a primary and a secondary FHIR record, newest first. Select a
                correlation id to open the side by side view.
            </caption>

            <thead>
                <tr>
                    <th scope="col">Compared</th>
                    <th scope="col">Correlation id</th>
                    <th scope="col">Differences</th>
                    <th scope="col">Breakdown</th>
                    <th scope="col">Accepted</th>
                    <th scope="col">State</th>
                    <th scope="col">Comment</th>
                    <th scope="col" className="text-end">Actions</th>
                </tr>
            </thead>

            <tbody>
                {comparisons.map(comparison => (
                    <tr
                        key={comparison.id}
                        className={comparison.id === selectedComparisonId
                            ? "table-active"
                            : undefined}
                        aria-current={comparison.id === selectedComparisonId
                            ? "true"
                            : undefined}>
                        <td className="text-nowrap">{comparison.comparedAtText}</td>

                        <th scope="row" className="fw-normal text-break">
                            <Link to={comparison.detailUrl}>{comparison.correlationId}</Link>
                        </th>

                        <td>
                            <span className={comparison.diffCountClassName}>
                                {comparison.diffCountText}
                            </span>
                        </td>

                        <td className="text-break">{comparison.breakdownText}</td>
                        <td>{comparison.acceptableDiffCountText}</td>

                        <td>
                            <span className={comparison.resolutionClassName}>
                                {comparison.resolutionText}
                            </span>
                        </td>

                        <td className="text-break">{comparison.commentText}</td>

                        <td className="text-end">
                            <Link
                                to={comparison.detailUrl}
                                className="btn btn-sm btn-outline-primary"
                                aria-label={`Compare ${comparison.correlationId}`}>
                                Compare
                            </Link>
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    );
}
