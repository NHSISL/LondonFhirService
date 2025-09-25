import axios from "axios";

export type FrontendConfigurationResponse = {
    clientId: string,
    authority: string,
    scopes: string,
}

export type FrontendConfiguration = {
    clientId: string,
    authority: string,
    scopes: string[],
}

class FrontendConfigurationBroker {
    relativeFrontendConfigurationUrl = '/api/FrontendConfigurations/';

    async GetFrontendConfigruationAsync(): Promise<FrontendConfiguration> {
        const url = `${this.relativeFrontendConfigurationUrl}`;

        try {
            const response = (await axios.get<FrontendConfigurationResponse>(url)).data;

            const result: FrontendConfiguration = {
                ...response,
                scopes: response.scopes.split(',')
            }

            if (!result.clientId ) {
                throw new Error("ClientId not provided");
            }

            if (!result.authority) {
                throw new Error("Authority not provided");
            }

            if (!result.scopes.length) {
                throw new Error("Scopes not provided");
            }

            return result;
        } catch (error) {
            console.error("Error fetching configuration", error);
            throw error;
        }
    }
}

export default FrontendConfigurationBroker;