import axios from 'axios';
import { BrowserAuthError, InteractionRequiredAuthError, PublicClientApplication } from "@azure/msal-browser";
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
            if (ApiBroker.isRecoverableBySigningIn(error)) {
                // fallback to interaction when silent call fails
                await this.msalInstance.acquireTokenRedirect(request);
            } else {
                console.log(error);
                throw error; // rethrow the error after logging it
            }
        }
        return authResult ? authResult.accessToken : null;
    }

    /**
     * Silent renewal failing is not the same as the caller being unable to sign in, and only one
     * of those was being retried. InteractionRequiredAuthError was handled; a BrowserAuthError -
     * most often monitor_window_timeout, the hidden renewal iframe not answering in time - was
     * rethrown, so the one failure that interaction would have fixed was the one failure that
     * never asked for it. The operator saw an error with no way forward and no amount of retrying
     * the same button would have helped.
     *
     * Only the codes a fresh sign-in actually resolves are listed. Redirecting on anything at all
     * risks a loop: acquireTokenRedirect navigates away, so a failure that survives the round trip
     * would send the operator round it again.
     */
    private static isRecoverableBySigningIn(error: unknown): boolean {
        if (error instanceof InteractionRequiredAuthError) {
            return true;
        }

        return error instanceof BrowserAuthError
            && [
                "monitor_window_timeout",
                "silent_prompt_value_error",
                "silent_sso_error",
                "no_account_error"
            ].includes(error.errorCode);
    }

    private async config() {
        const accessToken = await this.acquireAccessToken();
        if (accessToken) {
            return { headers: { 'Authorization': 'Bearer ' + accessToken } }
        }

        return {};
    }

    public async GetAsync(queryFragment: string, abortSignal?: AbortSignal) {
        const url = queryFragment;
        return axios.get(url, { ...await this.config(), signal: abortSignal });
    }

    public async GetAsyncAbsolute(absoluteUri: string) {
        return axios.get(absoluteUri, await this.config());;
    }

    public async PostAsync(relativeUrl: string, data: unknown, abortSignal?: AbortSignal) {
        const url = relativeUrl;

        return axios.post(url,
            data,
            { ...await this.config(), signal: abortSignal }
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