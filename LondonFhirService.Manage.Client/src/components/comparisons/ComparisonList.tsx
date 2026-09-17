import { Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { EmptyState } from "../shared/EmptyState";
import type { ComparisonListProps } from "../../models/components/comparisons/ComparisonListProps";

/**
 * An empty table used to say the same thing however it came to be empty - "try a different
 * correlation id" - which was wrong advice for the case that produces it most often. An operator
 * following the link off the structured record page arrives here within a second of the request
 * answering, holding a correlation id that certainly exists, before the comparison worker has been
 * round; being told to try a different one sends them looking for a fault that is not there.
 */
function describeEmptyList(
    searchTerm: string,
    pendingCount: number)
    : { title: string; message: string } {
    if (pendingCount > 0) {
        return {
            title: "Not compared yet",
            message: "The records above have arrived and are queued for the comparison service. "
                + "This page updates on its own, so they will appear here shortly."
        };
    }

    if (searchTerm.trim().length > 0) {
        return {
            title: "No comparisons found",
            message: "Nothing has been compared for this correlation id or comment, and nothing "
                + "matching it is waiting in the queue. Check the id, or clear the unresolved "
                + "filter."
        };
    }

    return {
        title: "No comparisons yet",
        message: "Nothing has been compared. A comparison appears here once a request has been "
            + "answered by more than one provider and the comparison service has checked them "
            + "against each other."
    };
}

export function ComparisonList({
    comparisons,
    selectedComparisonId,
    searchTerm = "",
    pendingCount = 0
}: ComparisonListProps) {
    if (comparisons.length === 0) {
        return <EmptyState {...describeEmptyList(searchTerm, pendingCount)} />;
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
                    <th scope="col">Source</th>
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

                        {/*
                            The source names the provider this row checked, so it is the cell an
                            operator reads to decide whether they want the row - and it went
                            nowhere, leaving the correlation id as the only way in. Same
                            destination, so either reading of "open this one" works.

                            The chip stays outside the link: it labels the source rather than
                            offering a second place to go.
                        */}
                        <td className="text-break">
                            <Link to={comparison.detailUrl}>{comparison.sourceNameText}</Link>
                            {comparison.isPrimarySource && (
                                <>
                                    {" "}
                                    <span
                                        className="badge bg-primary"
                                        title="This source is the primary">
                                        Primary
                                    </span>
                                </>
                            )}
                        </td>

                        <th scope="row" className="fw-normal text-break">
                            <Link to={comparison.detailUrl}>{comparison.correlationId}</Link>
                        </th>

                        <td>
                            <span className={comparison.diffCountClassName}>
                                {comparison.diffCountText}
                            </span>
                        </td>

                        <td className="text-break">{comparison.breakdownText}</td>
                        <td>
                            <span className={comparison.acceptableDiffCountClassName}>
                                {comparison.acceptableDiffCountText}
                            </span>
                        </td>

                        <td>
                            <span className={comparison.resolutionClassName}>
                                {comparison.resolutionText}
                            </span>
                        </td>

                        <td className="text-break">{comparison.commentText}</td>

                        <td className="text-end text-nowrap">
                            <Link
                                to={comparison.detailUrl}
                                className="btn btn-sm btn-outline-primary"
                                aria-label={`Compare ${comparison.correlationId}`}>
                                Compare
                            </Link>

                            {" "}

                            {/*
                                Whether the providers agreed is this table; how long they took is
                                the metrics screen. Labelled with the correlation id as well, because
                                every row here offers the same two words and a screen reader moving
                                between them would otherwise hear no difference.
                            */}
                            <Link
                                to={comparison.metricsUrl}
                                className="btn btn-sm btn-outline-secondary"
                                aria-label={`Show metrics for ${comparison.correlationId}`}>
                                Show metrics
                            </Link>
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    );
}
