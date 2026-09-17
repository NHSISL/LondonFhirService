import { PatientApiBrokerException } from "../../../models/foundations/patients/exceptions/PatientApiBrokerException";
import { PatientDependencyException } from "../../../models/foundations/patients/exceptions/PatientDependencyException";
import { PatientServiceException } from "../../../models/foundations/patients/exceptions/PatientServiceException";
import { PatientValidationException } from "../../../models/foundations/patients/exceptions/PatientValidationException";

export async function tryCatchPatientServiceAsync<T>(
    returningPatientFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningPatientFunction();
    } catch (exception) {
        if (exception instanceof PatientValidationException) {
            throw exception;
        }

        if (exception instanceof PatientApiBrokerException) {
            throw new PatientDependencyException(
                "Patient dependency error occurred, please contact support.",
                exception);
        }

        throw new PatientServiceException(
            "Patient service error occurred, please contact support.",
            exception);
    }
}
