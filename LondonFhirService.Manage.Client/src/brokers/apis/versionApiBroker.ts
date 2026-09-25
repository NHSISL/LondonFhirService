import ApiBroker from "../apiBroker";
import { VersionApiBrokerException } from "../../models/foundations/versions/exceptions/VersionApiBrokerException";
import type { ApplicationVersion } from "../../models/foundations/versions/ApplicationVersion";
import type { IVersionApiBroker } from "./iVersionApiBroker";

export class VersionApiBroker implements IVersionApiBroker {
    private readonly relativeVersionsUrl = "/api/versions";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async getVersionAsync(abortSignal?: AbortSignal): Promise<ApplicationVersion> {
        try {
            const response = await this.apiBroker.GetAsync(this.relativeVersionsUrl, abortSignal);
            const source = (response.data ?? {}) as Record<string, unknown>;

            // Read defensively: the API is an untyped boundary.
            return {
                coreVersion: typeof source.coreVersion === "string" ? source.coreVersion : ""
            };
        } catch (exception) {
            throw new VersionApiBrokerException("Failed to retrieve the version from the API.", exception);
        }
    }
}
