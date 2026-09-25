import type { ApplicationVersionView } from "../../../models/views/versions/ApplicationVersionView";

export interface IVersionViewService {
    retrieveVersionViewAsync(abortSignal?: AbortSignal): Promise<ApplicationVersionView>;
}
