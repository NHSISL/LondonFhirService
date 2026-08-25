import { Alert } from "react-bootstrap";
import type { ErrorSummaryProps } from "../../models/components/shared/ErrorSummaryProps";

export function ErrorSummary({ title, message }: ErrorSummaryProps) {
    return (
        <Alert variant="danger" role="alert">
            <Alert.Heading as="h2" className="h5">{title}</Alert.Heading>
            <p className="mb-0">{message}</p>
        </Alert>
    );
}
