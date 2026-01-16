import { Container, Row, Col } from "react-bootstrap";

export const Home = () => {
    return (
        <Container fluid className="mt-4">
            <Row className="mb-4 p-2">
                <Col>
                    <h3>The London Fhir Service Management Portal</h3>
                </Col>
            </Row>
        </Container>
    );
}