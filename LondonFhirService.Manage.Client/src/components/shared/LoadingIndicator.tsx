import { SpinnerBase } from "../bases/spinner/SpinnerBase";
import type { LoadingIndicatorProps } from "../../models/components/shared/LoadingIndicatorProps";

export function LoadingIndicator({ message }: LoadingIndicatorProps) {
    return (
        <div className="d-flex align-items-center gap-2" role="status" aria-live="polite">
            <SpinnerBase />
            <span>{message}</span>
        </div>
    );
}
