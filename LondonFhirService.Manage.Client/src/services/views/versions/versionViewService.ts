import { VersionService } from "../../foundations/versions/versionService";
import { VersionViewServiceException } from "../../../models/views/versions/exceptions/VersionViewServiceException";
import type { ApplicationVersionView } from "../../../models/views/versions/ApplicationVersionView";
import type { IVersionService } from "../../foundations/versions/iVersionService";
import type { IVersionViewService } from "./iVersionViewService";

export class VersionViewService implements IVersionViewService {
    private readonly versionService: IVersionService;

    constructor(versionService: IVersionService = new VersionService()) {
        this.versionService = versionService;
    }

    public async retrieveVersionViewAsync(abortSignal?: AbortSignal): Promise<ApplicationVersionView> {
        try {
            const applicationVersion = await this.versionService.retrieveVersionAsync(abortSignal);

            return { versionText: `v${applicationVersion.coreVersion.trim()}` };
        } catch (exception) {
            throw new VersionViewServiceException("We could not load the portal's version.", exception);
        }
    }
}
