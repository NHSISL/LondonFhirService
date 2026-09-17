import { Table } from "react-bootstrap";
import type { PendingComparisonListProps } from "../../models/components/comparisons/PendingComparisonListProps";

// Shown above the results, and only when there is something in it. A comparison is written by a
// worker on its own schedule, so between a request answering and the next tick the results table
// is empty through no fault of anyone's - this is what says so.
export function PendingComparisonList(
    { pendingComparisons, watching, error }: PendingComparisonListProps) {
    if (error) {
        return (
            <div className="alert alert-warning py-2 mb-3" role="status">
                The comparisons below are up to date, but we could not check what is still waiting
                to be compared. {error.message}
            </div>
        );
    }

    if (pendingComparisons.length === 0) {
        return null;
    }

    return (
        <div className="border rounded mb-3">
            <div className="d-flex align-items-center gap-2 px-3 pt-3">
                <h2 className="h6 mb-0">Waiting to be compared</h2>

                {watching && (
                    <span
                        className="spinner-border spinner-border-sm text-secondary"
                        role="status"
                        aria-hidden="true" />
                )}

                <span className="visually-hidden" role="status">
                    {watching ? "Checking for new comparisons" : ""}
                </span>
            </div>

            <p className="text-muted small px-3 mt-1 mb-2">
                These records have arrived from their providers and are queued for the comparison
                service. This list refreshes on its own, and each one moves into the table below as
                soon as it has been compared.
            </p>

            <Table responsive className="align-middle mb-0">
                <caption className="visually-hidden">
                    Records that have arrived but have not been compared yet, newest first.
                </caption>

                <thead>
                    <tr>
                        <th scope="col">Landed</th>
                        <th scope="col">Source</th>
                        <th scope="col">Correlation id</th>
                        <th scope="col">State</th>
                    </tr>
                </thead>

                <tbody>
                    {pendingComparisons.map(pendingComparison => (
                        <tr key={pendingComparison.id}>
                            <td className="text-nowrap">{pendingComparison.landedAtText}</td>

                            <td className="text-break">
                                {pendingComparison.sourceNameText}
                                {pendingComparison.isPrimarySource && (
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
                                {pendingComparison.correlationId}
                            </th>

                            <td>
                                <span className={pendingComparison.statusClassName}>
                                    {pendingComparison.statusText}
                                </span>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
}
