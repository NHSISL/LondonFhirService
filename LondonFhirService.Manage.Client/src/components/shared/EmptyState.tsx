import type { EmptyStateProps } from "../../models/components/shared/EmptyStateProps";

export function EmptyState({ title, message }: EmptyStateProps) {
    return (
        <div className="text-center text-muted border rounded p-4">
            <p className="fw-semibold mb-1">{title}</p>
            <p className="mb-0">{message}</p>
        </div>
    );
}
