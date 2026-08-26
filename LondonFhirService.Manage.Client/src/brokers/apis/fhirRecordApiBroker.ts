import ApiBroker from "../apiBroker";
import { FhirRecordApiBrokerException } from "../../models/foundations/fhirRecords/exceptions/FhirRecordApiBrokerException";
import { fhirRecordStatuses } from "../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecord, FhirRecordStatus } from "../../models/foundations/fhirRecords/FhirRecord";
import type { IFhirRecordApiBroker } from "./iFhirRecordApiBroker";

export class FhirRecordApiBroker implements IFhirRecordApiBroker {
    private readonly relativeFhirRecordsUrl = "/api/fhirrecords";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async getFhirRecordByIdAsync(
        fhirRecordId: string,
        abortSignal?: AbortSignal)
        : Promise<FhirRecord> {
        try {
            const response = await this.apiBroker.GetAsync(
                `${this.relativeFhirRecordsUrl}/${encodeURIComponent(fhirRecordId)}`,
                abortSignal);

            return this.toFhirRecord(response.data);
        } catch (exception) {
            throw new FhirRecordApiBrokerException(
                `Failed to retrieve FHIR record '${fhirRecordId}' from the API.`,
                exception);
        }
    }

    // Format conversion only - the API is an untyped boundary, so every field is read
    // defensively rather than asserted into shape.
    private toFhirRecord(rawFhirRecord: unknown): FhirRecord {
        if (typeof rawFhirRecord !== "object" || rawFhirRecord === null) {
            throw new Error("The FHIR records endpoint returned an unreadable record.");
        }

        const source = rawFhirRecord as Record<string, unknown>;

        return {
            id: this.readString(source.id),
            correlationId: this.readString(source.correlationId),
            jsonPayload: this.readString(source.jsonPayload),
            sourceName: this.readString(source.sourceName),
            isPrimarySource: source.isPrimarySource === true,
            isProcessed: source.isProcessed === true,
            status: this.readStatus(source.status),
            insertedDate: this.readString(source.insertedDate),
            createdBy: this.readString(source.createdBy),
            createdDate: this.readString(source.createdDate),
            updatedBy: this.readString(source.updatedBy),
            updatedDate: this.readString(source.updatedDate)
        };
    }

    // The host registers no JsonStringEnumConverter, so StatusType arrives as its ordinal. An
    // unknown value falls back to Pending rather than leaking a number the view layer cannot name.
    private readStatus(rawValue: unknown): FhirRecordStatus {
        const knownStatuses: number[] = Object.values(fhirRecordStatuses);

        return typeof rawValue === "number" && knownStatuses.includes(rawValue)
            ? rawValue as FhirRecordStatus
            : fhirRecordStatuses.pending;
    }

    private readString(rawValue: unknown): string {
        return typeof rawValue === "string" ? rawValue : "";
    }
}
