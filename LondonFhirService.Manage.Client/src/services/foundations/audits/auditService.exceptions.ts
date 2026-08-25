import { AuditApiBrokerException } from "../../../models/foundations/audits/exceptions/AuditApiBrokerException";
import { AuditDependencyException } from "../../../models/foundations/audits/exceptions/AuditDependencyException";
import { AuditServiceException } from "../../../models/foundations/audits/exceptions/AuditServiceException";
import { AuditValidationException } from "../../../models/foundations/audits/exceptions/AuditValidationException";

export async function tryCatchAuditServiceAsync<T>(
    returningAuditFunction: () => Promise<T>)
    : Promise<T> {
    try {
        return await returningAuditFunction();
    } catch (exception) {
        if (exception instanceof AuditValidationException) {
            throw exception;
        }

        if (exception instanceof AuditApiBrokerException) {
            throw new AuditDependencyException(
                "Audit dependency error occurred, please contact support.",
                exception);
        }

        throw new AuditServiceException(
            "Audit service error occurred, please contact support.",
            exception);
    }
}
