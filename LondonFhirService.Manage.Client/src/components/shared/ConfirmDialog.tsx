import { Button, Modal } from "react-bootstrap";
import type { ConfirmDialogProps } from "../../models/components/shared/ConfirmDialogProps";

export function ConfirmDialog({
    show,
    title,
    message,
    confirmLabel,
    confirmingLabel,
    confirming,
    onConfirm,
    onCancel
}: ConfirmDialogProps) {
    return (
        <Modal show={show} onHide={onCancel} centered backdrop="static">
            <Modal.Header closeButton>
                <Modal.Title as="h2" className="h5">{title}</Modal.Title>
            </Modal.Header>

            <Modal.Body>
                <p className="mb-0">{message}</p>
            </Modal.Body>

            <Modal.Footer>
                <Button variant="danger" onClick={onConfirm} disabled={confirming}>
                    {confirming ? confirmingLabel : confirmLabel}
                </Button>

                <Button variant="outline-secondary" onClick={onCancel} disabled={confirming}>
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
    );
}
