import { Container, Row, Col } from "react-bootstrap";
import TestComponent from "../components/testComponent/testComponent";

export const TestPage = () => {
    return (
        <Container fluid className="mt-4">
            <Row className="mb-4 p-2">
                <Col>
                    <TestComponent />
                </Col>
            </Row>
        </Container>
    );
}