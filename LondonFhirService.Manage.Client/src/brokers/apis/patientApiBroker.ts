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

    // The API's own explanation, not a generic message. PatientsController answers a validation
    // failure with a ProblemDetails naming the field that was wrong, and an upstream refusal or
    // failure with one whose detail is the token endpoint's or provider's own words. Discarding
    // either leaves an operator on a diagnostic page with nothing to diagnose.
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

        // detail before title, and the order matters. On an upstream refusal the title is this
        // service's own generic wrapper - "Patient dependency validation error occurred" - while
        // detail is the upstream's actual text. Reading title first would show the operator the
        // wrapper and throw away the only part that says what went wrong.
        for (const key of ["detail", "title", "message"]) {
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

    // Format conversion only. The endpoint returns the provider's payload as a bare string, and
    // what arrives here depends on the content type MVC picked: axios sends an Accept header, so
    // the JSON formatter wins and the body is a JSON-encoded string, which axios has already
    // parsed back into a string or - when the provider answered JSON - into an object. A client
    // that sends no Accept header gets text/plain instead. Both shapes are turned back into text
    // here, because the page shows what came back rather than reading fields out of it.
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
