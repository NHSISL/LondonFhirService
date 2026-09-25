import React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBars} from '@fortawesome/free-solid-svg-icons';
import { Button, Container, Navbar } from "react-bootstrap";
import Login from '../securitys/login';
import { useApplicationVersion } from '../../hooks/useApplicationVersion';

interface NavbarComponentProps {
    toggleSidebar: () => void;
    showMenuButton: boolean;
}

const NavbarComponent: React.FC<NavbarComponentProps> = ({ toggleSidebar, showMenuButton }) => {
    const { versionText } = useApplicationVersion();

    return (
        <Navbar className="bg-light" sticky="top">
            <Container fluid>
                {showMenuButton && (
                    <Button onClick={toggleSidebar} variant="outline-dark" className="ms-3">
                        <FontAwesomeIcon icon={faBars} />
                    </Button>
                )}
                <Navbar.Brand href="/" className="me-auto ms-3 d-flex align-items-center">
                    <span className="d-none d-md-inline" style={{ marginLeft: "10px" }}>
                       London FHIR Service Management Portal

                        {/* Inherits the brand's colour, so it reads as part of the title. */}
                        {versionText && <small className="ms-2">({versionText})</small>}
                    </span>
                </Navbar.Brand>
                <Navbar.Text>
                    <Login />
                </Navbar.Text>
            </Container>
        </Navbar>
    );
}

export default NavbarComponent;