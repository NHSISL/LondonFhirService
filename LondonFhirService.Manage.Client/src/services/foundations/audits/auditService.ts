import { AuditApiBroker } from "../../../brokers/apis/auditApiBroker";
import { tryCatchAuditServiceAsync } from "./auditService.exceptions";
import { validateAuditId, validateAuditQuery } from "./auditService.validations";
import type { Audit } from "../../../models/foundations/audits/Audit";
import type { AuditQuery } from "../../../models/foundations/audits/AuditQuery";
import type { IAuditApiBroker } from "../../../brokers/apis/iAuditApiBroker";
import type { IAuditService } from "./iAuditService";

export class AuditService implements IAuditService {
    private readonly auditApiBroker: IAuditApiBroker;

    constructor(auditApiBroker: IAuditApiBroker = new AuditApiBroker()) {
        this.auditApiBroker = auditApiBroker;
    }

    public async retrieveAuditsAsync(
        auditQuery: AuditQuery,
        abortSignal?: AbortSignal)
        : Promise<Audit[]> {
        return await tryCatchAuditServiceAsync(async () => {
            validateAuditQuery(auditQuery);

            return await this.auditApiBroker.getAuditsAsync(auditQuery, abortSignal);
        });
    }

    public async retrieveAuditByIdAsync(
        auditId: string,
        abortSignal?: AbortSignal)
        : Promise<Audit> {
        return await tryCatchAuditServiceAsync(async () => {
            validateAuditId(auditId);

            return await this.auditApiBroker.getAuditByIdAsync(auditId, abortSignal);
        });
    }
}
