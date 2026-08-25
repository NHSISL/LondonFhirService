import { Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { EmptyState } from "../shared/EmptyState";
import type { ProviderListProps } from "../../models/components/providers/ProviderListProps";

export function ProviderList({ providers, selectedProviderId }: ProviderListProps) {
    if (providers.length === 0) {
        return (
            <EmptyState
                title="No providers found"
                message="No provider matches your search. Try a different name, endpoint or status." />
        );
    }

    return (
        <Table hover responsive className="align-middle">
            <caption className="visually-hidden">
                Registered FHIR providers. Select a provider name to open its details.
            </caption>

            <thead>
                <tr>
                    <th scope="col">Provider</th>
                    <th scope="col">Fully qualified name</th>
                    <th scope="col">FHIR version</th>
                    <th scope="col">Role</th>
                    <th scope="col">Status</th>
                    <th scope="col">Active period</th>
                    <th scope="col" className="text-end">Actions</th>
                </tr>
            </thead>

            <tbody>
                {providers.map(provider => (
                    <tr
                        key={provider.id}
                        className={provider.id === selectedProviderId ? "table-active" : undefined}
                        aria-current={provider.id === selectedProviderId ? "true" : undefined}>
                        <th scope="row" className="fw-normal">
                            <Link to={provider.detailUrl}>
                                {provider.friendlyName}
                            </Link>
                        </th>
                        <td className="text-break">{provider.fullyQualifiedName}</td>
                        <td>{provider.fhirVersionText}</td>
                        <td>
                            <span className={provider.roleClassName}>{provider.roleText}</span>
                        </td>
                        <td>
                            <span className={provider.statusClassName}>{provider.statusText}</span>
                        </td>
                        <td>{provider.activePeriodText}</td>
                        <td className="text-end">
                            <Link
                                to={provider.detailUrl}
                                className="btn btn-sm btn-outline-primary"
                                aria-label={`View ${provider.friendlyName}`}>
                                View
                            </Link>
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    );
}
