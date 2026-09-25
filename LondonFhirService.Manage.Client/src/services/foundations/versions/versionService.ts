import { VersionApiBroker } from "../../../brokers/apis/versionApiBroker";
import { VersionValidationException } from "../../../models/foundations/versions/exceptions/VersionValidationException";
import { tryCatchVersionServiceAsync } from "./versionService.exceptions";
import type { ApplicationVersion } from "../../../models/foundations/versions/ApplicationVersion";
import type { IVersionApiBroker } from "../../../brokers/apis/iVersionApiBroker";
import type { IVersionService } from "./iVersionService";

export class VersionService implements IVersionService {
    private readonly versionApiBroker: IVersionApiBroker;

    constructor(versionApiBroker: IVersionApiBroker = new VersionApiBroker()) {
        this.versionApiBroker = versionApiBroker;
    }

    public async retrieveVersionAsync(abortSignal?: AbortSignal): Promise<ApplicationVersion> {
        return await tryCatchVersionServiceAsync(async () => {
            const applicationVersion = await this.versionApiBroker.getVersionAsync(abortSignal);

            // A blank version would render as "v" in the header, which reads as a bug.
            if (applicationVersion.coreVersion.trim().length === 0) {
                throw new VersionValidationException(
                    "coreVersion",
                    "The API did not report a version.");
            }

            return applicationVersion;
        });
    }
}
