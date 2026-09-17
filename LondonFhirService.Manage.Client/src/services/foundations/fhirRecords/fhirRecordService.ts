import { FhirRecordApiBroker } from "../../../brokers/apis/fhirRecordApiBroker";
import { tryCatchFhirRecordServiceAsync } from "./fhirRecordService.exceptions";
import { validateFhirRecordId, validateFhirRecordQuery } from "./fhirRecordService.validations";
import type { FhirRecord } from "../../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecordQuery } from "../../../models/foundations/fhirRecords/FhirRecordQuery";
import type { IFhirRecordApiBroker } from "../../../brokers/apis/iFhirRecordApiBroker";
import type { IFhirRecordService } from "./iFhirRecordService";

export class FhirRecordService implements IFhirRecordService {
    private readonly fhirRecordApiBroker: IFhirRecordApiBroker;

    constructor(fhirRecordApiBroker: IFhirRecordApiBroker = new FhirRecordApiBroker()) {
        this.fhirRecordApiBroker = fhirRecordApiBroker;
    }

    public async retrieveFhirRecordByIdAsync(
        fhirRecordId: string,
        abortSignal?: AbortSignal)
        : Promise<FhirRecord> {
        return await tryCatchFhirRecordServiceAsync(async () => {
            validateFhirRecordId(fhirRecordId);

            return await this.fhirRecordApiBroker
                .getFhirRecordByIdAsync(fhirRecordId, abortSignal);
        });
    }

    public async retrievePendingFhirRecordsAsync(
        fhirRecordQuery: FhirRecordQuery,
        abortSignal?: AbortSignal)
        : Promise<FhirRecord[]> {
        return await tryCatchFhirRecordServiceAsync(async () => {
            validateFhirRecordQuery(fhirRecordQuery);

            return await this.fhirRecordApiBroker
                .getPendingFhirRecordsAsync(fhirRecordQuery, abortSignal);
        });
    }
}
