import ApiBroker from "../apiBroker";
import { PatientApiBrokerException } from "../../models/foundations/patients/exceptions/PatientApiBrokerException";
import type { IPatientApiBroker } from "./iPatientApiBroker";
import type { StructuredRecordRequest } from "../../models/foundations/patients/StructuredRecordRequest";
import type { StructuredRecordResponse } from "../../models/foundations/patients/StructuredRecordResponse";

export class PatientApiBroker implements IPatientApiBroker {
    private readonly relativePatientsUrl = "/api/patients";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async postStructuredRecordAsync(
        structuredRecordRequest: StructuredRecordRequest,
        abortSignal?: AbortSignal)
        : Promise<StructuredRecordResponse> {
        try {
            const response = await this.apiBroker.PostAsync(
                `${this.relativePatientsUrl}/getstructuredrecord`,
                structuredRecordRequest,
                abortSignal);

            return {
                payloadText: this.toPayloadText(response.data),
                correlationId: this.readCorrelationId(response)
            };
        } catch (exception) {
            throw new PatientApiBrokerException(
                this.describeFailure(exception),
                exception,
                this.readFieldErrorBag(exception));
        }
    }

    // Header names are case insensitive on the wire and axios lowercases them, so this reads the
    // lowercase form whatever casing the host sent. Empty rather than undefined when it is absent -
    // an older build of the host sends no such header, and the page has to cope with not having it
    // rather than rendering a link to nowhere.
    private readCorrelationId(response: { headers?: unknown }): string {
        const headers = response?.headers as
            { [key: string]: unknown; get?: (name: string) => unknown } | undefined;

        if (headers === undefined || headers === null) {
            return "";
        }

        // axios gives an AxiosHeaders instance on a real call and a plain object in tests.
        const raw = typeof headers.get === "function"
            ? headers.get("x-correlation-id")
            : headers["x-correlation-id"];

        return typeof raw === "string" ? raw.trim() : "";
    }

    // The API's own explanation, not a generic message. PatientsController answers a validation
    // failure with a ProblemDetails naming the field that was wrong, and an upstream refusal or
    // failure with one whose detail is the token endpoint's or provider's own words. Discarding
    // either leaves an operator on a diagnostic page with nothing to diagnose.
    private describeFailure(exception: unknown): string {
        const failure = exception as {
            code?: string;
            message?: string;
            request?: unknown;
            response?: { status?: number; data?: unknown };
        };

        const response = failure?.response;

        if (response === undefined) {
            // Three different things used to share one sentence, and it asserted a network fact
            // none of them established. An axios error carries `request` once the call has gone
            // out, so its absence means we never got that far - a token we could not acquire, or
            // anything else thrown before the send.
            if (failure?.code === "ERR_CANCELED") {
                return "The request was cancelled before the API answered.";
            }

            const reason = failure?.message ? ` (${failure.message})` : "";

            return failure?.request === undefined
                ? `The request was never sent - the portal could not prepare it${reason}.`
                : `The API did not answer the request${reason}.`;
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

    /// The same bag readFieldErrors renders as prose, kept in its original shape so the page can
    /// put each message under the input it names. Read from the response rather than re-parsed
    /// out of the message, because a field whose own text contains a colon or a semicolon cannot
    /// be recovered from the flattened string.
    private readFieldErrorBag(exception: unknown): Record<string, string[]> {
        const rawBody = (exception as { response?: { data?: unknown } })?.response?.data;

        if (typeof rawBody !== "object" || rawBody === null) {
            return {};
        }

        const rawErrors = (rawBody as Record<string, unknown>).errors;

        if (typeof rawErrors !== "object" || rawErrors === null) {
            return {};
        }

        const fieldErrors: Record<string, string[]> = {};

        for (const [field, messages] of Object.entries(rawErrors as Record<string, unknown>)) {
            const messageList = Array.isArray(messages)
                ? messages.map(message => String(message))
                : [String(messages)];

            // camelCased on the way in regardless of how the server cased it, so a page keyed by
            // its own form field names does not depend on the host's serialiser settings.
            fieldErrors[field.charAt(0).toLowerCase() + field.slice(1)] = messageList;
        }

        return fieldErrors;
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
    // verbatim - axios's default Accept header ends in */*, and MvcOptions
    // .RespectBrowserAcceptHeader is false, so MVC disregards that header and writes the string
    // through StringOutputFormatter. axios then parses a body that happens to be valid JSON before
    // this ever sees it, so the response arrives as an object when the provider answered JSON and
    // as a string when it did not. Both are turned back into text here, because the page shows
    // what came back rather than reading fields out of it.
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
