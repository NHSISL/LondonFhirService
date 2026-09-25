import { expect, it } from "vitest";
import { FileDownloadService } from "./fileDownloadService";
import { FileDownloadDependencyException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadDependencyException";
import { FileDownloadBrokerException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadBrokerException";
import { FileDownloadValidationException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadValidationException";
import type { IFileDownloadBroker } from "../../../brokers/iFileDownloadBroker";

it("should hand the file to the broker", async () => {
    const downloads: { fileName: string; content: Blob }[] = [];
    const content = new Blob(["a,b"], { type: "text/csv" });

    const fileDownloadService = new FileDownloadService({
        downloadFileAsync: async (fileName, blob) => { downloads.push({ fileName, content: blob }); }
    });

    await fileDownloadService.downloadFileAsync("metrics.csv", content);

    expect(downloads).toEqual([{ fileName: "metrics.csv", content }]);
});

it("should reject a blank file name without touching the broker", async () => {
    let called = false;

    const fileDownloadService = new FileDownloadService({
        downloadFileAsync: async () => { called = true; }
    });

    await expect(fileDownloadService.downloadFileAsync(" ", new Blob()))
        .rejects.toBeInstanceOf(FileDownloadValidationException);

    expect(called).toBe(false);
});

it("should report a broker failure as a dependency error", async () => {
    const failingBroker: IFileDownloadBroker = {
        downloadFileAsync: async () => {
            throw new FileDownloadBrokerException("blocked", new Error("blocked"));
        }
    };

    await expect(new FileDownloadService(failingBroker).downloadFileAsync("metrics.csv", new Blob()))
        .rejects.toBeInstanceOf(FileDownloadDependencyException);
});
