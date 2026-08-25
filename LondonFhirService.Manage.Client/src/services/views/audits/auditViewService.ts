import moment from "moment";
import { AuditService } from "../../foundations/audits/auditService";
import { AuditViewServiceException } from "../../../models/views/audits/exceptions/AuditViewServiceException";
import type { Audit } from "../../../models/foundations/audits/Audit";
import type { AuditDetailView } from "../../../models/views/audits/AuditDetailView";
import type { AuditListItemView } from "../../../models/views/audits/AuditListItemView";
import type { AuditPageView } from "../../../models/views/audits/AuditPageView";
import type { IAuditService } from "../../foundations/audits/iAuditService";
import type { IAuditViewService } from "./iAuditViewService";

const notSetText = "—";
const dateDisplayFormat = "DD MMM YYYY HH:mm:ss";

export const auditPageSize = 50;

export class AuditViewService implements IAuditViewService {
    private readonly auditService: IAuditService;

    constructor(auditService: IAuditService = new AuditService()) {
        this.auditService = auditService;
    }

    public async retrieveAuditPageViewAsync(
        pageNumber: number,
        searchTerm: string,
        abortSignal?: AbortSignal)
        : Promise<AuditPageView> {
        try {
            const audits = await this.auditService.retrieveAuditsAsync(
                {
                    skip: pageNumber * auditPageSize,
                    take: auditPageSize,
                    searchTerm: searchTerm
                },
                abortSignal);

            return {
                audits: audits.map(audit => this.toAuditListItemView(audit)),

                // The endpoint does not report a total, so a full page is taken as a signal that
                // there may be another one. A short page is the end.
                hasMore: audits.length === auditPageSize
            };
        } catch (exception) {
            throw new AuditViewServiceException(
                "We could not load the audits, please try again or contact support.",
                exception);
        }
    }

    public async retrieveAuditDetailViewAsync(
        auditId: string,
        abortSignal?: AbortSignal)
        : Promise<AuditDetailView> {
        try {
            const audit = await this.auditService.retrieveAuditByIdAsync(auditId, abortSignal);

            return this.toAuditDetailView(audit);
        } catch (exception) {
            throw new AuditViewServiceException(
                "We could not load this audit, please try again or contact support.",
                exception);
        }
    }

    private toAuditListItemView(audit: Audit): AuditListItemView {
        return {
            id: audit.id,
            createdDateText: this.formatDate(audit.createdDate),
            auditTypeText: audit.auditType || notSetText,
            title: audit.title || notSetText,
            logLevelText: this.mapLogLevelToDisplayText(audit.logLevel),
            logLevelClassName: this.mapLogLevelToClassName(audit.logLevel),
            correlationIdText: audit.correlationId ?? notSetText,
            createdByText: audit.createdBy || notSetText,
            detailUrl: this.buildDetailUrl(audit.id)
        };
    }

    private toAuditDetailView(audit: Audit): AuditDetailView {
        return {
            id: audit.id,
            title: audit.title || notSetText,
            auditTypeText: audit.auditType || notSetText,
            message: audit.message || notSetText,
            logLevelText: this.mapLogLevelToDisplayText(audit.logLevel),
            logLevelClassName: this.mapLogLevelToClassName(audit.logLevel),
            correlationIdText: audit.correlationId ?? notSetText,
            fileNameText: audit.fileName ?? notSetText,
            createdByText: audit.createdBy || notSetText,
            createdDateText: this.formatDate(audit.createdDate),
            updatedByText: audit.updatedBy || notSetText,
            updatedDateText: this.formatDate(audit.updatedDate),
            detailUrl: this.buildDetailUrl(audit.id)
        };
    }

    private mapLogLevelToDisplayText(logLevel: string): string {
        return logLevel || "Information";
    }

    private mapLogLevelToClassName(logLevel: string): string {
        const logLevelClassNames: Record<string, string> = {
            "Critical": "badge bg-danger",
            "Error": "badge bg-danger",
            "Warning": "badge bg-warning text-dark",
            "Information": "badge bg-info text-dark",
            "Debug": "badge bg-secondary",
            "Trace": "badge bg-light text-dark border"
        };

        return logLevelClassNames[logLevel] ?? "badge bg-secondary";
    }

    private formatDate(value: string): string {
        if (!value || value.length === 0) {
            return notSetText;
        }

        const parsedValue = moment(value);

        return parsedValue.isValid() ? parsedValue.format(dateDisplayFormat) : notSetText;
    }

    private buildDetailUrl(auditId: string): string {
        return `/admin/audits/${encodeURIComponent(auditId)}`;
    }
}
