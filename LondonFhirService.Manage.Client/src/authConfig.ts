/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { LogLevel } from '@azure/msal-browser';
import { Configuration, RedirectRequest } from "@azure/msal-browser";
import FrontendConfigurationBroker from './brokers/apiBroker.frontendConfigurationBroker';


/**
 * Configuration object to be passed to MSAL instance on creation. 
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md 
 */

//const config = await new FrontendConfigurationBroker().GetFrontendConfigruationAsync()


export class MsalConfig {

    // static msalConfig: Configuration;
    private static _loginRequest?: RedirectRequest;

    private static _msalConfig?: Configuration;

    static get msalConfig(): Configuration {
        if (!this._msalConfig) {
            throw 'MSALConfig has not been built, ensure MsalConfig.Build has been run.'
        }
        return this._msalConfig;
    }

    static get loginRequest(): RedirectRequest {
        if (!this._loginRequest) {
            throw 'MSALConfig has not been built, ensure MsalConfig.Build has been run.'
        }
        return this._loginRequest;
    }

    static async build() {
        const configurationBroker = new FrontendConfigurationBroker();
        const remoteConfiguration = await configurationBroker.GetFrontendConfigruationAsync();

        /**
         * Scopes you add here will be prompted for user consent during sign-in.
         * By default, MSAL.js will add OIDC scopes (openid, profile, email) to any login request.
         * For more information about OIDC scopes, visit: 
         * https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
         */
        this._loginRequest = {
            scopes: remoteConfiguration.scopes
        }

        this._msalConfig = {
            auth: {
                clientId: remoteConfiguration.clientId, // This is the ONLY mandatory field that you need to supply.
                authority: remoteConfiguration.authority, // Replace the placeholder with your tenant subdomain 
                redirectUri: '/', // Points to window.location.origin. You must register this URI on Azure Portal/App Registration.
                postLogoutRedirectUri: '/', // Indicates the page to navigate after logout.
                navigateToLoginRequestUrl: false, // If "true", will navigate back to the original request location before processing the auth code response.
            },
            cache: {
                cacheLocation: 'localStorage', // Configures cache location. "sessionStorage" is more secure, but "localStorage" gives you SSO between tabs.
                storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
            },
            system: {
                loggerOptions: {
                    loggerCallback: (level, message, containsPii) => {
                        if (containsPii) {
                            return;
                        }
                        switch (level) {
                            case LogLevel.Error:
                                console.error(message);
                                return;
                            case LogLevel.Info:
                                console.info(message);
                                return;
                            case LogLevel.Verbose:
                                console.debug(message);
                                return;
                            case LogLevel.Warning:
                                console.warn(message);
                                return;
                            default:
                                return;
                        }
                    }
                }
            }
        }
    }
};



/*export class LoginRequest {
    static staticConfig?: FrontendConfiguration;
    static async build(): Promise<PopupRequest> {
        if (!this.staticConfig) {
            const configurationBroker = new FrontendConfigurationBroker();
            this.staticConfig = await configurationBroker.GetFrontendConfigruationAsync();
        }

        return {
            scopes: this.staticConfig.scopes
        }
    }
}*/
/*export const loginRequest: PopupRequest = {
    scopes: config.scopes
};*/

/**
 * An optional silentRequest object can be used to achieve silent SSO
 * between applications by providing a "login_hint" property.
 */
// export const silentRequest = {
//     scopes: ["openid", "profile"],
//     loginHint: "example@domain.net"
// };
