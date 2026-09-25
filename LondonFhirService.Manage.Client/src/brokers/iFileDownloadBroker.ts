export interface IFileDownloadBroker {
    downloadFileAsync(fileName: string, content: Blob): Promise<void>;
}
