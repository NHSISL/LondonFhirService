import { FhirRecordDifferenceApiBroker } from "../../../brokers/apis/fhirRecordDifferenceApiBroker";
import { tryCatchFhirRecordDifferenceServiceAsync } from "./fhirRecordDifferenceService.exceptions";
import {
    validateFhirRecordDifferenceId,
    validateFhirRecordDifferenceModification,
    validateFhirRecordDifferenceQuery
} from "./fhirRecordDifferenceService.validations";
import type { FhirRecordDifference } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifference";
import type { FhirRecordDifferenceQuery } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";
import type { IFhirRecordDifferenceApiBroker } from "../../../brokers/apis/iFhirRecordDifferenceApiBroker";
import type { IFhirRecordDifferenceService } from "./iFhirRecordDifferenceService";

export class FhirRecordDifferenceService implements IFhirRecordDifferenceService {
    private readonly fhirRecordDifferenceApiBroker: IFhirRecordDifferenceApiBroker;

    constructor(
        fhirRecordDifferenceApiBroker: IFhirRecordDifferenceApiBroker =
        new FhirRecordDifferenceApiBroker()) {
        this.fhirRecordDifferenceApiBroker = fhirRecordDifferenceApiBroker;
    }

    public async retrieveFhirRecordDifferencesAsync(
        fhirRecordDifferenceQuery: FhirRecordDifferenceQuery,
        abortSignal?: AbortSignal)
        : Promise<FhirRecordDifference[]> {
        return await tryCatchFhirRecordDifferenceServiceAsync(async () => {
            validateFhirRecordDifferenceQuery(fhirRecordDifferenceQuery);

            return await this.fhirRecordDifferenceApiBroker
                .getFhirRecordDifferencesAsync(fhirRecordDifferenceQuery, abortSignal);
        });
    }

    public async retrieveFhirRecordDifferenceByIdAsync(
        fhirRecordDifferenceId: string,
        abortSignal?: AbortSignal)
        : Promise<FhirRecordDifference> {
        return await tryCatchFhirRecordDifferenceServiceAsync(async () => {
            validateFhirRecordDifferenceId(fhirRecordDifferenceId);

            return await this.fhirRecordDifferenceApiBroker
                .getFhirRecordDifferenceByIdAsync(fhirRecordDifferenceId, abortSignal);
        });
    }

    public async modifyFhirRecordDifferenceAsync(
        fhirRecordDifference: FhirRecordDifference)
        : Promise<FhirRecordDifference> {
        return await tryCatchFhirRecordDifferenceServiceAsync(async () => {
            validateFhirRecordDifferenceModification(fhirRecordDifference);

            return await this.fhirRecordDifferenceApiBroker
                .putFhirRecordDifferenceAsync(fhirRecordDifference);
        });
    }
}
