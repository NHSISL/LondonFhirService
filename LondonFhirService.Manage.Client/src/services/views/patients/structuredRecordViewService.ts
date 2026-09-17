import { PatientService } from "../../foundations/patients/patientService";
import { PatientValidationException } from "../../../models/foundations/patients/exceptions/PatientValidationException";
import { StructuredRecordViewServiceException } from "../../../models/views/patients/exceptions/StructuredRecordViewServiceException";
import type { IPatientService } from "../../foundations/patients/iPatientService";
import type { IStructuredRecordViewService } from "./iStructuredRecordViewService";
import type { StructuredRecordFormValues } from "../../../models/views/patients/StructuredRecordFormValues";
import type { StructuredRecordRequest } from "../../../models/foundations/patients/StructuredRecordRequest";
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
            const payloadText = await this.patientService.retrieveStructuredRecordAsync(
                this.toStructuredRecordRequest(structuredRecordFormValues),
                abortSignal);

            return this.toStructuredRecordView(payloadText);
        } catch (exception) {
            // A field the operator can fix is reported as itself. Wrapping it in the generic
            // "contact support" message would tell someone who mistyped a date to raise a ticket.
            if (exception instanceof PatientValidationException) {
                throw exception;
            }

            throw new StructuredRecordViewServiceException(
                "We could not retrieve the structured record, please try again or contact support.",
                exception);
        }
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
    private toStructuredRecordView(payloadText: string): StructuredRecordView {
        const formattedPayload = this.formatPayload(payloadText);

        return {
            payloadText: formattedPayload.text,
            isJson: formattedPayload.isJson,
            lineCount: formattedPayload.text.length === 0
                ? 0
                : formattedPayload.text.split("\n").length,
            characterCountText: `${formattedPayload.text.length.toLocaleString()} characters`
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
