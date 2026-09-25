import { expect, it } from "vitest";
import { VersionViewService } from "./versionViewService";
import { VersionService } from "../../foundations/versions/versionService";
import { VersionViewServiceException } from "../../../models/views/versions/exceptions/VersionViewServiceException";
import { VersionValidationException } from "../../../models/foundations/versions/exceptions/VersionValidationException";
import type { IVersionApiBroker } from "../../../brokers/apis/iVersionApiBroker";

const brokerReturning = (coreVersion: string): IVersionApiBroker => ({
    getVersionAsync: async () => ({ coreVersion })
});

it("should show the core version with a v in front", async () => {
    const versionViewService =
        new VersionViewService(new VersionService(brokerReturning(" 0.13.0.0 ")));

    const versionView = await versionViewService.retrieveVersionViewAsync();

    expect(versionView.versionText).toBe("v0.13.0.0");
});

it("should refuse a blank version rather than show a bare v", async () => {
    await expect(new VersionService(brokerReturning(" ")).retrieveVersionAsync())
        .rejects.toBeInstanceOf(VersionValidationException);

    await expect(new VersionViewService(new VersionService(brokerReturning(""))).retrieveVersionViewAsync())
        .rejects.toBeInstanceOf(VersionViewServiceException);
});
