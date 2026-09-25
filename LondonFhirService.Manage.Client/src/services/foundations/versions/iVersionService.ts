import type { ApplicationVersion } from "../../../models/foundations/versions/ApplicationVersion";

export interface IVersionService {
    retrieveVersionAsync(abortSignal?: AbortSignal): Promise<ApplicationVersion>;
}
