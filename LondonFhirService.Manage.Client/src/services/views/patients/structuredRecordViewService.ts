import { PatientApiBrokerException } from "../../../models/foundations/patients/exceptions/PatientApiBrokerException";
import { buildComparisonsUrl, buildMetricsUrl } from "../../../helpers/correlationIds";
import { PatientService } from "../../foundations/patients/patientService";
import { PatientValidationException } from "../../../models/foundations/patients/exceptions/PatientValidationException";
import {
    StructuredRecordViewServiceException
} from "../../../models/views/patients/exceptions/StructuredRecordViewServiceException";
import type { IPatientService } from "../../foundations/patients/iPatientService";
import type { IStructuredRecordViewService } from "./iStructuredRecordViewService";
import type { StructuredRecordFormValues } from "../../../models/views/patients/StructuredRecordFormValues";
import type { StructuredRecordRequest } from "../../../models/foundations/patients/StructuredRecordRequest";
import type { StructuredRecordResponse } from "../../../models/foundations/patients/StructuredRecordResponse";
import type { StructuredRecordView } from "../../../models/views/patients/StructuredRecordView";

// The host falls back to its own configured grant type when this is blank, but client_credentials
// is what every consumer of this endpoint uses, so the field is pre-filled rather than left empty
// for an operator to guess at.
const defaultGrantType = "client_credentials";

export class StructuredRecordViewService implements IStructuredRecordViewService {
    private readonly patientService: IPatientService;

    constructor(patientService: IPatientService = new PatientService()) {
        this.patientService = patientService;
    }

    public createStructuredRecordFormValues(): StructuredRecordFormValues {
        return {
            clientId: "",
            clientSecret: "",
            scope: "",
            grantType: defaultGrantType,
            nhsNumber: "",
            dateOfBirth: "",
            demographicsOnly: false
        };
    }

    public async retrieveStructuredRecordViewAsync(
        structuredRecordFormValues: StructuredRecordFormValues,
        abortSignal?: AbortSignal)
        : Promise<StructuredRecordView> {
        try {
            const structuredRecordResponse = await this.patientService
                .retrieveStructuredRecordAsync(
                    this.toStructuredRecordRequest(structuredRecordFormValues),
                    abortSignal);

            return this.toStructuredRecordView(structuredRecordResponse);
        } catch (exception) {
            // A field the operator can fix is reported as itself. Wrapping it in the generic
            // "contact support" message would tell someone who mistyped a date to raise a ticket.
            if (exception instanceof PatientValidationException) {
                throw exception;
            }

            throw new StructuredRecordViewServiceException(
                this.describeFailure(exception),
                exception);
        }
    }

    // The broker records what the API actually said. Surfacing it here is the difference between
    // an operator learning that the NHS number was rejected and being told to contact support
    // about a page whose whole job is telling them what happened.
    private describeFailure(exception: unknown): string {
        const apiDetail = this.findApiBrokerMessage(exception);

        return apiDetail === null
            ? "We could not retrieve the structured record, please try again or contact support."
            : `We could not retrieve the structured record. ${apiDetail}`;
    }

    private findApiBrokerMessage(exception: unknown): string | null {
        let current: unknown = exception;

        // Bounded rather than while-true: these chains are three deep, and a cycle in someone
        // else's exception should not hang the page.
        for (let depth = 0; depth < 5; depth++) {
            if (current === null || current === undefined) {
                return null;
            }

            if (current instanceof PatientApiBrokerException) {
                return current.message;
            }

            current = (current as { innerException?: unknown }).innerException;
        }

        return null;
    }

    // Trimmed on the way out, because a credential pasted from a password manager routinely
    // carries a trailing space - and a blank field has to reach the server genuinely blank for
    // the fallback to configuration to apply.
    private toStructuredRecordRequest(
        structuredRecordFormValues: StructuredRecordFormValues)
        : StructuredRecordRequest {
        return {
            clientId: structuredRecordFormValues.clientId.trim(),
            clientSecret: structuredRecordFormValues.clientSecret.trim(),
            scope: structuredRecordFormValues.scope.trim(),
            grantType: structuredRecordFormValues.grantType.trim(),
            nhsNumber: structuredRecordFormValues.nhsNumber.trim(),
            dateOfBirth: structuredRecordFormValues.dateOfBirth.trim(),
            demographicsOnly: structuredRecordFormValues.demographicsOnly
        };
    }

    // Pretty printed when it parses, shown as it arrived when it does not. A provider answering
    // something that is not JSON is exactly the case an operator opened this page to see, so it
    // must stay visible rather than becoming an error.
    private toStructuredRecordView(
        structuredRecordResponse: StructuredRecordResponse): StructuredRecordView {
        const formattedPayload = this.formatPayload(structuredRecordResponse.payloadText);
        const correlationId = structuredRecordResponse.correlationId ?? "";

        return {
            payloadText: formattedPayload.text,
            isJson: formattedPayload.isJson,
            lineCount: formattedPayload.text.length === 0
                ? 0
                : formattedPayload.text.split("\n").length,
            characterCountText: `${formattedPayload.text.length.toLocaleString()} characters`,

            // Empty when the host sent no header, which is what the page checks before offering
            // the links - there is nothing to link to without it.
            correlationId: correlationId,

            // Both blank rather than pointing at a route with no id, which would land on the
            // metrics list or the unfiltered comparisons table and read as "we found everything"
            // when the truth is that this call was not filed under anything.
            metricsUrl: correlationId.length === 0 ? "" : buildMetricsUrl(correlationId),

            comparisonsUrl:
                correlationId.length === 0 ? "" : buildComparisonsUrl(correlationId)
        };
    }

    private formatPayload(payloadText: string): { text: string; isJson: boolean } {
        const trimmedPayload = payloadText?.trim() ?? "";

        if (trimmedPayload.length === 0) {
            return { text: "", isJson: false };
        }

        try {
            return {
                text: JSON.stringify(JSON.parse(trimmedPayload), null, 2),
                isJson: true
            };
        } catch {
            return { text: payloadText, isJson: false };
        }
    }
}
