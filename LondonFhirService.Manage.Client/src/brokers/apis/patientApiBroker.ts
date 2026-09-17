import ApiBroker from "../apiBroker";
import { PatientApiBrokerException } from "../../models/foundations/patients/exceptions/PatientApiBrokerException";
import type { IPatientApiBroker } from "./iPatientApiBroker";
import type { StructuredRecordRequest } from "../../models/foundations/patients/StructuredRecordRequest";

export class PatientApiBroker implements IPatientApiBroker {
    private readonly relativePatientsUrl = "/api/patients";
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
                `${this.relativePatientsUrl}/getstructuredrecord`,
                structuredRecordRequest,
                abortSignal);

            return this.toPayloadText(response.data);
        } catch (exception) {
            throw new PatientApiBrokerException(this.describeFailure(exception), exception);
        }
    }

    // The API's own explanation, not a generic message. A 400 from PatientsController is a
    // ProblemDetails naming the field that was wrong, and a 500 carries what the provider said
    // when it refused. Discarding either leaves an operator on a diagnostic page with nothing to
    // diagnose.
    private describeFailure(exception: unknown): string {
        const response =
            (exception as { response?: { status?: number; data?: unknown } })?.response;

        if (response === undefined) {
            return "The structured record request did not reach the API.";
        }

        const detail = this.readDetail(response.data);

        return detail.length > 0
            ? `The API answered ${response.status}: ${detail}`
            : `The API answered ${response.status}.`;
    }

    private readDetail(rawBody: unknown): string {
        if (typeof rawBody === "string") {
            return rawBody.trim();
        }

        if (typeof rawBody !== "object" || rawBody === null) {
            return "";
        }

        const body = rawBody as Record<string, unknown>;

        // The field keyed bag first: it names what the operator got wrong, which is the most
        // actionable thing the API ever sends back.
        const fieldErrors = this.readFieldErrors(body.errors);

        if (fieldErrors.length > 0) {
            return fieldErrors;
        }

        for (const key of ["title", "message", "detail"]) {
            const value = body[key];

            if (typeof value === "string" && value.trim().length > 0) {
                return value.trim();
            }
        }

        return "";
    }

    private readFieldErrors(rawErrors: unknown): string {
        if (typeof rawErrors !== "object" || rawErrors === null) {
            return "";
        }

        return Object.entries(rawErrors as Record<string, unknown>)
            .map(([field, messages]) =>
                `${field}: ${Array.isArray(messages) ? messages.join(", ") : String(messages)}`)
            .join("; ");
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
