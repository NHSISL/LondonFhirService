import axios from 'axios';
import { InteractionRequiredAuthError, PublicClientApplication } from "@azure/msal-browser";
import { MsalConfig } from '../authConfig';

class ApiBroker {
    msalInstance = new PublicClientApplication(MsalConfig.msalConfig);
    scope: string[];

    constructor(scope?: string) {
        this.scope = scope ? [scope] : MsalConfig.loginRequest.scopes;
    }


    private async initialize() {
        await this.msalInstance.initialize();
    }

    private async acquireAccessToken() {
        await this.initialize(); // Ensure MSAL is initialized

        const activeAccount = this.msalInstance.getActiveAccount();
        const accounts = this.msalInstance.getAllAccounts();

        const request = {
            scopes: this.scope,
            account: activeAccount || accounts[0]
        };

        let authResult;
        try {
            authResult = await this.msalInstance.acquireTokenSilent(request);
        } catch (error) {
            if (error instanceof InteractionRequiredAuthError) {
                // fallback to interaction when silent call fails
                await this.msalInstance.acquireTokenRedirect(request);
            } else {
                console.log(error);
                throw error; // rethrow the error after logging it
            }
        }
        return authResult ? authResult.accessToken : null;
    }

    private async config() {
        const accessToken = await this.acquireAccessToken();
        if (accessToken) {
            return { headers: { 'Authorization': 'Bearer ' + accessToken } }
        }

        return {};
    }

    public async GetAsync(queryFragment: string) {
        const url = queryFragment;
        return axios.get(url, await this.config());
    }

    public async GetAsyncAbsolute(absoluteUri: string) {
        return axios.get(absoluteUri, await this.config());;
    }

    public async PostAsync(relativeUrl: string, data: unknown) {
        const url = relativeUrl;

        return axios.post(url,
            data,
            await this.config()
        );
    }

    public async PostFormAsync(relativeUrl: string, data: FormData) {
        const url = relativeUrl;

        const headers = {
            'Authorization': 'Bearer ' + await this.acquireAccessToken(),
            "Content-Type": 'multipart/form-data'
        }

        return axios.post(url,
            data,
            { headers }
        );
    }

    public async PutAsync(relativeUrl: string, data: unknown) {
        const url = relativeUrl;

        return axios.put(url, data, await this.config());
    }

    public async DeleteAsync(relativeUrl: string) {
        const url = relativeUrl;

        return axios.delete(url, await this.config());
    }
}

export default ApiBroker;