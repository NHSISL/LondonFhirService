import { Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { EmptyState } from "../shared/EmptyState";
import type { MetricListProps } from "../../models/components/metrics/MetricListProps";

export function MetricList({ metrics }: MetricListProps) {
    if (metrics.length === 0) {
        return (
            <EmptyState
                title="No requests found"
                message="No request has been measured yet, or none is retained within the current window." />
        );
    }

    return (
        <Table hover responsive className="align-middle">
            <caption className="visually-hidden">
                Measured requests, newest first. Select View to see every span of a request.
            </caption>

            <thead>
                <tr>
                    <th scope="col">Started</th>
                    <th scope="col">Correlation id</th>
                    <th scope="col">Method</th>
                    <th scope="col">Name</th>
                    <th scope="col">Status</th>
                    <th scope="col" className="text-end">Duration</th>
                    <th scope="col">Consumer</th>
                    <th scope="col">User</th>
                    <th scope="col" className="text-end">Actions</th>
                </tr>
            </thead>

            <tbody>
                {metrics.map(metric => (
                    <tr key={metric.id}>
                        <td className="text-nowrap">{metric.startedText}</td>

                        {/* The row header, because the correlation id is what identifies this
                            request - the method is the same string on every row. */}
                        <th scope="row" className="fw-normal text-break">
                            <Link to={metric.detailUrl}>{metric.correlationId}</Link>
                        </th>

                        <td className="text-break">{metric.methodText}</td>
                        <td className="text-break">{metric.nameText}</td>
                        <td>
                            <span className={metric.statusClassName}>{metric.statusText}</span>
                        </td>
                        <td className="text-end text-nowrap">{metric.durationText}</td>
                        <td className="text-break">{metric.consumerText}</td>
                        <td className="text-break">{metric.userIdText}</td>
                        <td className="text-end">
                            <Link
                                to={metric.detailUrl}
                                className="btn btn-sm btn-outline-primary"
                                aria-label={`View the spans of the request started ${metric.startedText}`}>
                                View
                            </Link>
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    );
}
