import ApiBroker from "../apiBroker";
import { FhirRecordApiBrokerException } from "../../models/foundations/fhirRecords/exceptions/FhirRecordApiBrokerException";
import { buildPendingFhirRecordQueryUrl } from "./fhirRecordApiBroker.queries";
import { fhirRecordStatuses } from "../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecord, FhirRecordStatus } from "../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecordQuery } from "../../models/foundations/fhirRecords/FhirRecordQuery";
import type { IFhirRecordApiBroker } from "./iFhirRecordApiBroker";

export class FhirRecordApiBroker implements IFhirRecordApiBroker {
    private readonly relativeFhirRecordsUrl = "/api/fhirrecords";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async getPendingFhirRecordsAsync(
        fhirRecordQuery: FhirRecordQuery,
        abortSignal?: AbortSignal)
        : Promise<FhirRecord[]> {
        try {
            const response = await this.apiBroker.GetAsync(
                buildPendingFhirRecordQueryUrl(this.relativeFhirRecordsUrl, fhirRecordQuery),
                abortSignal);

            const rawFhirRecords = Array.isArray(response.data) ? response.data : [];

            return rawFhirRecords.map(rawFhirRecord => this.toFhirRecord(rawFhirRecord));
        } catch (exception) {
            throw new FhirRecordApiBrokerException(
                "Failed to retrieve the pending FHIR records from the API.",
                exception);
        }
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
    //
    // A record read by id arrives whole; one read through the pending query arrives as an OData
    // projection carrying only the fields that query asked for. A field the projection left out
    // reads as its empty value rather than as a failure - the pending list shows no payload, so it
    // must not require one.
    private toFhirRecord(rawFhirRecord: unknown): FhirRecord {
        if (typeof rawFhirRecord !== "object" || rawFhirRecord === null) {
            throw new Error("The FHIR records endpoint returned an unreadable record.");
        }

        const source = this.readFields(rawFhirRecord);

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

    /**
     * Field names lowercased before they are read. Both shapes this endpoint answers with are
     * camelCase today - the unprojected one by the host's naming policy, the projected one by
     * OData's wrapper, which an acceptance test pins - so this normalises nothing in practice. It
     * stays because the whole of this class treats the response as untyped and reads every field
     * defensively, and because the one thing that must not happen here is a silent row of blanks.
     */
    private readFields(rawValue: unknown): Record<string, unknown> {
        if (typeof rawValue !== "object" || rawValue === null) {
            return {};
        }

        return Object.fromEntries(
            Object.entries(rawValue as Record<string, unknown>)
                .map(([name, value]) => [
                    name.charAt(0).toLowerCase() + name.slice(1),
                    value
                ]));
    }

    // The host registers no JsonStringEnumConverter, so StatusType arrives as its ordinal -
    // through a projection too, which an acceptance test pins. An unknown value falls back to
    // Pending rather than leaking a number the view layer cannot name.
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
