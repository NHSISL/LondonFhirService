import ApiBroker from "../apiBroker";
import { AuditApiBrokerException } from "../../models/foundations/audits/exceptions/AuditApiBrokerException";
import { buildAuditQueryUrl } from "./auditApiBroker.queries";
import type { Audit } from "../../models/foundations/audits/Audit";
import type { AuditQuery } from "../../models/foundations/audits/AuditQuery";
import type { IAuditApiBroker } from "./iAuditApiBroker";

export class AuditApiBroker implements IAuditApiBroker {
    private readonly relativeAuditsUrl = "/api/audits";
    private readonly apiBroker: ApiBroker;

    constructor(apiBroker: ApiBroker = new ApiBroker()) {
        this.apiBroker = apiBroker;
    }

    public async getAuditsAsync(
        auditQuery: AuditQuery,
        abortSignal?: AbortSignal)
        : Promise<Audit[]> {
        try {
            const response =
                await this.apiBroker.GetAsync(
                    buildAuditQueryUrl(this.relativeAuditsUrl, auditQuery),
                    abortSignal);

            const rawAudits: unknown = response.data;

            if (Array.isArray(rawAudits) === false) {
                throw new Error("The audits endpoint did not return a collection.");
            }

            return (rawAudits as unknown[]).map(rawAudit => this.toAudit(rawAudit));
        } catch (exception) {
            throw new AuditApiBrokerException(
                "Failed to retrieve audits from the API.",
                exception);
        }
    }

    public async getAuditByIdAsync(auditId: string, abortSignal?: AbortSignal): Promise<Audit> {
        try {
            const response = await this.apiBroker.GetAsync(
                `${this.relativeAuditsUrl}/${encodeURIComponent(auditId)}`,
                abortSignal);

            return this.toAudit(response.data);
        } catch (exception) {
            throw new AuditApiBrokerException(
                `Failed to retrieve audit '${auditId}' from the API.`,
                exception);
        }
    }

    private toAudit(rawAudit: unknown): Audit {
        if (typeof rawAudit !== "object" || rawAudit === null) {
            throw new Error("The audits endpoint returned an unreadable audit.");
        }

        const source = rawAudit as Record<string, unknown>;

        return {
            id: this.readString(source.id),
            correlationId: this.readNullableString(source.correlationId),
            auditType: this.readString(source.auditType),
            title: this.readString(source.title),
            message: this.readString(source.message),
            fileName: this.readNullableString(source.fileName),
            logLevel: this.readString(source.logLevel),
            createdBy: this.readString(source.createdBy),
            createdDate: this.readString(source.createdDate),
            updatedBy: this.readString(source.updatedBy),
            updatedDate: this.readString(source.updatedDate)
        };
    }

    private readString(rawValue: unknown): string {
        return typeof rawValue === "string" ? rawValue : "";
    }

    private readNullableString(rawValue: unknown): string | null {
        return typeof rawValue === "string" && rawValue.length > 0 ? rawValue : null;
    }
}
