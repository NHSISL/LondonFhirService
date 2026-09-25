export interface IFileDownloadService {
    downloadFileAsync(fileName: string, content: Blob): Promise<void>;
}
