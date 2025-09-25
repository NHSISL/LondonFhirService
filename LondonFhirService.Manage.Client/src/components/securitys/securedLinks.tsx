import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import { ReactElement } from 'react';
import { Link } from 'react-router-dom';

type SecuredLinkParameters = {
    to: string,
    children: string,
    allowedRoles?: Array<string>,
    deniedRoles?: Array<string>
}

export const SecuredLink = ({ to, children, allowedRoles = [], deniedRoles = [] }: SecuredLinkParameters): ReactElement => {

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
        return <span>
            <Link to={to} className="text-white">
                {children}
            </Link>
        </span>
    }

    return <></>;
}
