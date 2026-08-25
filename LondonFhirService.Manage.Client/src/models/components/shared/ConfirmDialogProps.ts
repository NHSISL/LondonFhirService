export type ConfirmDialogProps = {
    show: boolean;
    title: string;
    message: string;
    confirmLabel: string;
    confirmingLabel: string;
    confirming: boolean;
    onConfirm: () => void;
    onCancel: () => void;
};
