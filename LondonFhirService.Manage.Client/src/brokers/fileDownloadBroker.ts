import { FileDownloadBrokerException } from "../models/foundations/fileDownloads/exceptions/FileDownloadBrokerException";
import type { IFileDownloadBroker } from "./iFileDownloadBroker";

// The browser is an external dependency like any other: this is the one place that reaches for
// object URLs and the document to hand a file to the person using the page.
export class FileDownloadBroker implements IFileDownloadBroker {
    public async downloadFileAsync(fileName: string, content: Blob): Promise<void> {
        let objectUrl: string | undefined;

        try {
            objectUrl = URL.createObjectURL(content);
            const anchor = document.createElement("a");
            anchor.href = objectUrl;
            anchor.download = fileName;
            document.body.appendChild(anchor);
            anchor.click();
            anchor.remove();
        } catch (exception) {
            throw new FileDownloadBrokerException("Failed to download the file.", exception);
        } finally {
            // Released once the click has handed the file to the browser; holding it would keep
            // the whole export in memory for the life of the page.
            if (objectUrl !== undefined) {
                URL.revokeObjectURL(objectUrl);
            }
        }
    }
}
