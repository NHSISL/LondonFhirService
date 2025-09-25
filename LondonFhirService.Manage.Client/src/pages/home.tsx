import { Container, Row, Col } from "react-bootstrap";

export const Home = () => {
    return (
        <Container fluid className="mt-4">
            <Row className="mb-4 p-2">
                <Col>
                    <h3>The London Data Service Opt Out Management Portal</h3>
                    <p>
                        The London Data Service securely collects patient information from a range of healthcare locations around
                        London such as GP surgeries and hospitals.
                        It then organises and stores this information ready to be used by other approved systems.
                    </p>
                    
                </Col>
            </Row>
        </Container>
    );
}