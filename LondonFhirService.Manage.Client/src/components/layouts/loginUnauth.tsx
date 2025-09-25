import { useMsal } from "@azure/msal-react";
import { Button, Container, Row, Col, Card } from "react-bootstrap";
import { MsalConfig } from '../../authConfig';

export const LoginUnAuthorisedComponent = () => {

    const { instance } = useMsal();

    const handleLoginRedirect = () => {
        instance.loginRedirect(MsalConfig.loginRequest).catch((error) => console.log(error));
    };

    return (
        <Container className="d-flex justify-content-center align-items-center" style={{ height: '100vh' }}>
            <Row>
                <Col>
                    <Card className="text-center" style={{ maxWidth: '400px', margin: 'auto' }}>
                        <Card.Body>
                            <Card.Title className="mb-4 ">
                                <img src="/OneLondon_Logo_OneLondon_Logo_Blue.png" alt="London Data Service logo" height="70" width="216" />
                                <br />
                                <span style={{ marginLeft: "10px" }}>
                                    London Data Service <br />
                                    <strong className="hero-text"> Local Data Opt-Out</strong>
                                </span>

                            </Card.Title>
                            <Card.Text className="mb-4 align-items-left" >
                                <p>Welcome to the One London Local Data Opt-Out Management Portal.</p>
                                <p>Please sign in to continue.</p>
                            </Card.Text>
                            <Button onClick={handleLoginRedirect} className="me-3">Sign in</Button>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}

export default LoginUnAuthorisedComponent;