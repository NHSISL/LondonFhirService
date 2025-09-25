import { useMsal } from "@azure/msal-react";
import { ReactElement, useState } from "react"
import { Button, Card, ListGroup, Modal, NavDropdown } from "react-bootstrap";


export const UserProfile = (): ReactElement => {
    const { accounts } = useMsal();
    const [showModal, setShowModal] = useState(false);
    const closeModal = () => setShowModal(false);
    const openModal = () => setShowModal(true);

    return (
        <div>
            <Modal show={showModal} onHide={closeModal} size="lg" centered>
                <Modal.Header closeButton>
                    <Modal.Title>My Profile</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Card>
                        <Card.Body>
                            <ListGroup variant="flush">
                                <ListGroup.Item>
                                    <div className="d-flex justify-content-between align-items-center">
                                        <div className="fw-bold">Username / Email</div>
                                        <div>{accounts[0]?.username}</div>
                                    </div>
                                </ListGroup.Item>
                                <ListGroup.Item>
                                    <div className="d-flex justify-content-between align-items-center">
                                        <div className="fw-bold">Name</div>
                                        <div>{accounts[0]?.name}</div>
                                    </div>
                                </ListGroup.Item>
                                {accounts[0]?.idTokenClaims?.roles?.map((r, i) => (
                                    <ListGroup.Item key={i}>
                                        <div className="d-flex justify-content-between align-items-center">
                                            <div className="fw-bold">Role</div>
                                            <div>{r}</div>
                                        </div>
                                    </ListGroup.Item>
                                ))}
                            </ListGroup>
                        </Card.Body>
                    </Card>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="danger" onClick={closeModal}>
                        Close
                    </Button>
                </Modal.Footer>
            </Modal>

            <NavDropdown.Item onClick={openModal}>My Profile</NavDropdown.Item>
        </div>
    );
};