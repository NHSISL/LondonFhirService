import { FileDownloadBroker } from "../../../brokers/fileDownloadBroker";
import { tryCatchFileDownloadServiceAsync } from "./fileDownloadService.exceptions";
import { validateFileDownload } from "./fileDownloadService.validations";
import type { IFileDownloadBroker } from "../../../brokers/iFileDownloadBroker";
import type { IFileDownloadService } from "./iFileDownloadService";

export class FileDownloadService implements IFileDownloadService {
    private readonly fileDownloadBroker: IFileDownloadBroker;

    constructor(fileDownloadBroker: IFileDownloadBroker = new FileDownloadBroker()) {
        this.fileDownloadBroker = fileDownloadBroker;
    }

    public async downloadFileAsync(fileName: string, content: Blob): Promise<void> {
        return await tryCatchFileDownloadServiceAsync(async () => {
            validateFileDownload(fileName, content);

            await this.fileDownloadBroker.downloadFileAsync(fileName, content);
        });
    }
}
