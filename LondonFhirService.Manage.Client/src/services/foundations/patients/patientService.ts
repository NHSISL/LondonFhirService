import { PatientApiBroker } from "../../../brokers/apis/patientApiBroker";
import { tryCatchPatientServiceAsync } from "./patientService.exceptions";
import { validateStructuredRecordRequest } from "./patientService.validations";
import type { IPatientApiBroker } from "../../../brokers/apis/iPatientApiBroker";
import type { IPatientService } from "./iPatientService";
import type { StructuredRecordRequest } from "../../../models/foundations/patients/StructuredRecordRequest";
import type { StructuredRecordResponse } from "../../../models/foundations/patients/StructuredRecordResponse";

export class PatientService implements IPatientService {
    private readonly patientApiBroker: IPatientApiBroker;

    constructor(patientApiBroker: IPatientApiBroker = new PatientApiBroker()) {
        this.patientApiBroker = patientApiBroker;
    }

    public async retrieveStructuredRecordAsync(
        structuredRecordRequest: StructuredRecordRequest,
        abortSignal?: AbortSignal)
        : Promise<StructuredRecordResponse> {
        return await tryCatchPatientServiceAsync(async () => {
            validateStructuredRecordRequest(structuredRecordRequest);

            return await this.patientApiBroker.postStructuredRecordAsync(
                structuredRecordRequest,
                abortSignal);
        });
    }
}
