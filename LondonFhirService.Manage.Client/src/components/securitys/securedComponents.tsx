import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { ReactElement } from 'react';

type SecuredComponentsParameters = {
    children: ReactElement,
    allowedRoles?: Array<string>,
    deniedRoles?: Array<string>
}


export const SecuredComponent = ({ children, allowedRoles = [], deniedRoles = [] }: SecuredComponentsParameters): ReactElement => {
    const { accounts } = useMsal();
    const isAuthenticated = useIsAuthenticated();

    const userRoles = (): Array<string> => {
        if (accounts.length && accounts[0].idTokenClaims && accounts[0].idTokenClaims.roles) {
            return accounts[0].idTokenClaims.roles;
        }

        return []
    }

    const userIsInRole = (roles: Array<string>): boolean => {
        let found = false;
        roles.forEach(r => {
            if (userRoles().indexOf(r) > -1) {
                found = true;
            }
        });
        return found;
    }

    if (isAuthenticated && userIsInRole(deniedRoles)) {
        return <></>
    }

    if (isAuthenticated && (allowedRoles.length === 0 || userIsInRole(allowedRoles))) {
        return <>
            {children}
        </>
    }

    return <></>;
}