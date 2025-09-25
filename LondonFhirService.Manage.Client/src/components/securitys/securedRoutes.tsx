import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { ReactElement } from 'react';
import { Alert, Button, Container } from 'react-bootstrap';
import { MsalConfig } from '../../authConfig';

type SecuredRouteParameters = {
    children: ReactElement,
    allowedRoles?: Array<string>,
    deniedRoles?: Array<string>
}

export const SecuredRoute = ({ children, allowedRoles = [], deniedRoles = [] }: SecuredRouteParameters): ReactElement => {
    const isAuthenticated = useIsAuthenticated();
    const { accounts, instance } = useMsal();


    const userRoles = () => {
        if (accounts.length && accounts[0].idTokenClaims && accounts[0].idTokenClaims.roles) {
            return accounts[0].idTokenClaims.roles;
        }

        return []
    }

    const userIsInRole = (roles: Array<string>) => {
        let found = false;
        roles.forEach(r => {
            if (userRoles().indexOf(r) > -1) {
                found = true;
            }
        });
        return found;
    }

    const NoAccess = () => {
        return <Container fluid className="mt-3">
            <Alert variant="danger">
                <Alert.Heading>Invalid Access</Alert.Heading>
                <p>
                    You do not have access to this area of the application, please contact support.
                </p>
                {!isAuthenticated && (
                    <Button className="inlineLogin" onClick={() => instance.loginRedirect(MsalConfig.loginRequest)}>
                        Login
                    </Button>
                )}
            </Alert>
        </Container>
    }

    if (isAuthenticated && userIsInRole(deniedRoles)) {
        return <NoAccess />;
    }

    if (isAuthenticated && (allowedRoles.length === 0 || userIsInRole(allowedRoles))) {
        return <>
            {children}
        </>
    }

    if (isAuthenticated) {
        return <NoAccess />
    }

    return <Container className="mt-3">
        <div>
            <span className="sr-only" id="deniedAccessReason">
                You are not logged in.
            </span>
            <Alert variant="warning">
                <Alert.Heading>Access Restricted</Alert.Heading>
                <p>
                    To access this part of the site, you must first login.
                </p>
                <Button className="inlineLoginNotAuth" onClick={() => instance.loginRedirect(MsalConfig.loginRequest)}>
                    Login
                </Button>
            </Alert>
        </div>
    </Container>
}