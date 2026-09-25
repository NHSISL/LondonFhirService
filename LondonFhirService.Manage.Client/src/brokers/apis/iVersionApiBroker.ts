import type { ApplicationVersion } from "../../models/foundations/versions/ApplicationVersion";

export interface IVersionApiBroker {
    getVersionAsync(abortSignal?: AbortSignal): Promise<ApplicationVersion>;
}
