import { FileDownloadValidationException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadValidationException";

export function validateFileDownload(fileName: string, content: Blob): void {
    if (typeof fileName !== "string" || fileName.trim().length === 0) {
        throw new FileDownloadValidationException("fileName", "A file name is required.");
    }

    if (content === null || content === undefined) {
        throw new FileDownloadValidationException("content", "The file content is required.");
    }
}
