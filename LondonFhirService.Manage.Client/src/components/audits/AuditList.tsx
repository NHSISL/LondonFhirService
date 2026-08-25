import { Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { EmptyState } from "../shared/EmptyState";
import type { AuditListProps } from "../../models/components/audits/AuditListProps";

export function AuditList({ audits, selectedAuditId }: AuditListProps) {
    if (audits.length === 0) {
        return (
            <EmptyState
                title="No audits found"
                message="No audit matches your search. Try a different title, type or correlation id." />
        );
    }

    return (
        <Table hover responsive className="align-middle">
            <caption className="visually-hidden">
                Audit records, newest first. Select an audit title to open its details.
            </caption>

            <thead>
                <tr>
                    <th scope="col">Created</th>
                    <th scope="col">Level</th>
                    <th scope="col">Type</th>
                    <th scope="col">Title</th>
                    <th scope="col">Correlation id</th>
                    <th scope="col">Created by</th>
                    <th scope="col" className="text-end">Actions</th>
                </tr>
            </thead>

            <tbody>
                {audits.map(audit => (
                    <tr
                        key={audit.id}
                        className={audit.id === selectedAuditId ? "table-active" : undefined}
                        aria-current={audit.id === selectedAuditId ? "true" : undefined}>
                        <td className="text-nowrap">{audit.createdDateText}</td>
                        <td>
                            <span className={audit.logLevelClassName}>{audit.logLevelText}</span>
                        </td>
                        <td>{audit.auditTypeText}</td>
                        <th scope="row" className="fw-normal">
                            <Link to={audit.detailUrl}>{audit.title}</Link>
                        </th>
                        <td className="text-break">{audit.correlationIdText}</td>
                        <td className="text-break">{audit.createdByText}</td>
                        <td className="text-end">
                            <Link
                                to={audit.detailUrl}
                                className="btn btn-sm btn-outline-primary"
                                aria-label={`View ${audit.title}`}>
                                View
                            </Link>
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    );
}
