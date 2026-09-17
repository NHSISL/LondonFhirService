import ApiBroker from "../apiBroker";
import { FhirRecordDifferenceApiBrokerException } from "../../models/foundations/fhirRecordDifferences/exceptions/FhirRecordDifferenceApiBrokerException";
import { buildFhirRecordDifferenceQueryUrl } from "./fhirRecordDifferenceApiBroker.queries";
import type { FhirRecordDifference } from "../../models/foundations/fhirRecordDifferences/FhirRecordDifference";
import type { FhirRecordDifferenceQuery } from "../../models/foundations/fhirRecordDifferences/FhirRecordDifferenceQuery";
import type { IFhirRecordDifferenceApiBroker } from "./iFhirRecordDifferenceApiBroker";

export class FhirRecordDifferenceApiBroker implements IFhirRecordDifferenceApiBroker {
    private readonly relativeFhirRecordDifferencesUrl = "/api/fhirrecorddifferences";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async getFhirRecordDifferencesAsync(
        fhirRecordDifferenceQuery: FhirRecordDifferenceQuery,
        abortSignal?: AbortSignal)
        : Promise<FhirRecordDifference[]> {
        try {
            const response = await this.apiBroker.GetAsync(
                buildFhirRecordDifferenceQueryUrl(
                    this.relativeFhirRecordDifferencesUrl,
                    fhirRecordDifferenceQuery),
                abortSignal);

            const rawFhirRecordDifferences: unknown = response.data;

            if (Array.isArray(rawFhirRecordDifferences) === false) {
                throw new Error(
                    "The FHIR record differences endpoint did not return a collection.");
            }

            return (rawFhirRecordDifferences as unknown[])
                .map(rawFhirRecordDifference =>
                    this.toFhirRecordDifference(rawFhirRecordDifference));
        } catch (exception) {
            throw new FhirRecordDifferenceApiBrokerException(
                "Failed to retrieve FHIR record differences from the API.",
                exception);
        }
    }

    public async getFhirRecordDifferenceByIdAsync(
        fhirRecordDifferenceId: string,
        abortSignal?: AbortSignal)
        : Promise<FhirRecordDifference> {
        try {
            const response = await this.apiBroker.GetAsync(
                `${this.relativeFhirRecordDifferencesUrl}/` +
                `${encodeURIComponent(fhirRecordDifferenceId)}`,
                abortSignal);

            return this.toFhirRecordDifference(response.data);
        } catch (exception) {
            throw new FhirRecordDifferenceApiBrokerException(
                `Failed to retrieve FHIR record difference '${fhirRecordDifferenceId}' ` +
                "from the API.",
                exception);
        }
    }

    public async putFhirRecordDifferenceAsync(
        fhirRecordDifference: FhirRecordDifference)
        : Promise<FhirRecordDifference> {
        try {
            const response = await this.apiBroker.PutAsync(
                this.relativeFhirRecordDifferencesUrl,
                fhirRecordDifference);

            return this.toFhirRecordDifference(response.data);
        } catch (exception) {
            throw new FhirRecordDifferenceApiBrokerException(
                `Failed to update FHIR record difference '${fhirRecordDifference.id}' ` +
                "through the API.",
                exception);
        }
    }

    // Format conversion only - the API is an untyped boundary, so every field is read
    // defensively rather than asserted into shape.
    private toFhirRecordDifference(rawFhirRecordDifference: unknown): FhirRecordDifference {
        if (typeof rawFhirRecordDifference !== "object" || rawFhirRecordDifference === null) {
            throw new Error(
                "The FHIR record differences endpoint returned an unreadable difference.");
        }

        const source = this.readFields(rawFhirRecordDifference);
        const secondary = this.readFields(source.secondary);

        return {
            id: this.readString(source.id),
            primaryId: this.readString(source.primaryId),
            secondaryId: this.readString(source.secondaryId),
            correlationId: this.readString(source.correlationId),
            diffJson: this.readString(source.diffJson),
            diffCount: this.readNumber(source.diffCount),
            acceptableDiffCount: this.readNumber(source.acceptableDiffCount),
            comparedAt: this.readString(source.comparedAt),
            comment: this.readNullableString(source.comment),
            isResolved: source.isResolved === true,
            secondarySourceName: this.readString(secondary.sourceName),
            secondaryIsPrimarySource: secondary.isPrimarySource === true,
            createdBy: this.readString(source.createdBy),
            createdDate: this.readString(source.createdDate),
            updatedBy: this.readString(source.updatedBy),
            updatedDate: this.readString(source.updatedDate)
        };
    }

    /**
     * Field names lowercased, so the same reader handles both shapes this endpoint answers with.
     * Without a query option it serialises through the host's camelCase policy; ask for $expand or
     * $select and OData's projection wrapper takes over and writes PascalCase instead. Reading one
     * casing meant the other arrived as a row of empty strings rather than as an error.
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

    private readString(rawValue: unknown): string {
        return typeof rawValue === "string" ? rawValue : "";
    }

    private readNullableString(rawValue: unknown): string | null {
        return typeof rawValue === "string" && rawValue.length > 0 ? rawValue : null;
    }

    private readNumber(rawValue: unknown): number {
        return typeof rawValue === "number" && Number.isFinite(rawValue) ? rawValue : 0;
    }
}
