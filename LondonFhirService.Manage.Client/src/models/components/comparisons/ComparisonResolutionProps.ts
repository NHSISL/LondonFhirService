import type { ComparisonDetailView } from "../../views/comparisons/ComparisonDetailView";
import type { ComparisonFormErrors } from "../../views/comparisons/ComparisonFormErrors";
import type { ComparisonFormValues } from "../../views/comparisons/ComparisonFormValues";

export type ComparisonResolutionProps = {
    comparison: ComparisonDetailView;
    editing: boolean;
    saving: boolean;
    saveError: Error | null;
    values: ComparisonFormValues;
    errors: ComparisonFormErrors;
    onEdit: () => void;
    onFieldChange: (fieldName: keyof ComparisonFormValues, value: string | boolean) => void;
    onSave: () => void;
    onCancelEdit: () => void;
};
