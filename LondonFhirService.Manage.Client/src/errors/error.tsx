import { Alert } from "react-bootstrap";
import { isRouteErrorResponse, useRouteError } from "react-router-dom";

export default function ErrorPage(): JSX.Element {
    const error = useRouteError();
    let errorMessage: string;

    if (isRouteErrorResponse(error)) {
        // error is type `ErrorResponse`
        errorMessage = error.data?.message || error.statusText;
    } else if (error instanceof Error) {
        errorMessage = error.message;
    } else if (typeof error === 'string') {
        errorMessage = error;
    } else {
        console.error('Unknown error:', error);
        errorMessage = 'An unknown error has occurred';
    }

    return (
        <div id="error-page">
            <Alert variant="danger">
                <Alert.Heading><h1>Oops!</h1></Alert.Heading>
                <p>Sorry, an unexpected error has occurred.</p>
                <p>
                    <i>{errorMessage}</i>
                </p>
            </Alert>
        </div>
    );
}