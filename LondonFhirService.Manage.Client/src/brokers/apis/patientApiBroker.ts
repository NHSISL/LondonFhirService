import ApiBroker from "../apiBroker";
import { PatientApiBrokerException } from "../../models/foundations/patients/exceptions/PatientApiBrokerException";
import type { IPatientApiBroker } from "./iPatientApiBroker";
import type { StructuredRecordRequest } from "../../models/foundations/patients/StructuredRecordRequest";

export class PatientApiBroker implements IPatientApiBroker {
    private readonly relativePatientUrl = "/api/patient";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async postStructuredRecordAsync(
        structuredRecordRequest: StructuredRecordRequest,
        abortSignal?: AbortSignal)
        : Promise<string> {
        try {
            const response = await this.apiBroker.PostAsync(
                `${this.relativePatientUrl}/getstructuredrecord`,
                structuredRecordRequest,
                abortSignal);

            return this.toPayloadText(response.data);
        } catch (exception) {
            throw new PatientApiBrokerException(
                "Failed to retrieve the structured record from the API.",
                exception);
        }
    }

    // Format conversion only. The endpoint answers text/plain carrying the provider's payload
    // verbatim, but axios parses a body that happens to be valid JSON before this ever sees it -
    // so the response arrives as an object when the provider answered JSON and as a string when
    // it did not. Both are turned back into text here, because the page shows what came back
    // rather than reading fields out of it.
    private toPayloadText(rawPayload: unknown): string {
        if (typeof rawPayload === "string") {
            return rawPayload;
        }

        if (rawPayload === null || rawPayload === undefined) {
            return "";
        }

        return JSON.stringify(rawPayload, null, 2);
    }
}
