import { Container, Row, Col } from "react-bootstrap";

export const TestComponent = () => {
    return (
        <Container fluid>
            <Row className="justify-content-center">
                <Col md={12} lg={12} xl={12}>
                  This is a test Component
                </Col>
            </Row>
        </Container>
    );
};

export default TestComponent;