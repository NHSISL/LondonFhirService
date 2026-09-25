import { FileDownloadBrokerException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadBrokerException";
import { FileDownloadDependencyException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadDependencyException";
import { FileDownloadServiceException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadServiceException";
import { FileDownloadValidationException } from "../../../models/foundations/fileDownloads/exceptions/FileDownloadValidationException";

export async function tryCatchFileDownloadServiceAsync<T>(
    returningFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningFunction();
    } catch (exception) {
        if (exception instanceof FileDownloadValidationException) {
            throw exception;
        }

        if (exception instanceof FileDownloadBrokerException) {
            throw new FileDownloadDependencyException(
                "File download dependency error occurred, please contact support.",
                exception);
        }

        throw new FileDownloadServiceException(
            "File download service error occurred, please contact support.",
            exception);
    }
}
