import { AuditValidationException } from "../../../models/foundations/audits/exceptions/AuditValidationException";
import type { AuditQuery } from "../../../models/foundations/audits/AuditQuery";

export function validateAuditId(auditId: string): void {
    if (auditId === null || auditId === undefined) {
        throw new AuditValidationException("auditId", "An audit id is required.");
    }

    if (auditId.trim().length === 0) {
        throw new AuditValidationException("auditId", "An audit id cannot be blank.");
    }
}

export function validateAuditQuery(auditQuery: AuditQuery): void {
    if (auditQuery === null || auditQuery === undefined) {
        throw new AuditValidationException("auditQuery", "An audit query is required.");
    }

    if (Number.isInteger(auditQuery.skip) === false || auditQuery.skip < 0) {
        throw new AuditValidationException("skip", "Skip must be a whole number of zero or more.");
    }

    if (Number.isInteger(auditQuery.take) === false || auditQuery.take < 1) {
        throw new AuditValidationException("take", "Take must be a whole number of one or more.");
    }
}
