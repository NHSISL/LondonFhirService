import { expect, it } from "vitest";
import { AuditViewService, auditPageSize } from "./auditViewService";
import type { Audit } from "../../../models/foundations/audits/Audit";
import type { AuditQuery } from "../../../models/foundations/audits/AuditQuery";
import type { IAuditService } from "../../foundations/audits/iAuditService";

const createAudit = (overrides: Partial<Audit>): Audit => ({
    id: "1b0f6a2e-4c31-4a8f-9a1d-77c2c0d51000",
    correlationId: "corr-1",
    auditType: "PatientAccess",
    title: "Structured record retrieved",
    message: "Retrieved a structured record for a patient.",
    fileName: null,
    logLevel: "Information",
    createdBy: "528f3bb2-27b4-40d8-a694-5e78bfd3480e",
    createdDate: "2026-08-25T16:14:00+00:00",
    updatedBy: "528f3bb2-27b4-40d8-a694-5e78bfd3480e",
    updatedDate: "2026-08-25T16:14:00+00:00",
    ...overrides
});

const createAuditService = (overrides: Partial<IAuditService> = {}): IAuditService => ({
    retrieveAuditsAsync: async () => [],
    retrieveAuditByIdAsync: async () => createAudit({}),
    ...overrides
});

it("should ask for the requested page and map the rows for display", async () => {
    let captured: AuditQuery | null = null;

    const auditViewService = new AuditViewService(createAuditService({
        retrieveAuditsAsync: async auditQuery => {
            captured = auditQuery;

            return [createAudit({ logLevel: "Error" })];
        }
    }));

    const auditPageView = await auditViewService.retrieveAuditPageViewAsync(2, "  patient  ");

    const auditQuery = captured as unknown as AuditQuery;
    expect(auditQuery.skip).toBe(2 * auditPageSize);
    expect(auditQuery.take).toBe(auditPageSize);
    expect(auditQuery.searchTerm).toBe("  patient  ");

    expect(auditPageView.audits[0].logLevelText).toBe("Error");
    expect(auditPageView.audits[0].logLevelClassName).toBe("badge bg-danger");
    expect(auditPageView.audits[0].detailUrl)
        .toBe("/admin/audits/1b0f6a2e-4c31-4a8f-9a1d-77c2c0d51000");
});

it("should report more pages only when the page came back full", async () => {
    const fullPage = Array.from({ length: auditPageSize }, () => createAudit({}));

    const withFullPage = new AuditViewService(createAuditService({
        retrieveAuditsAsync: async () => fullPage
    }));

    const withShortPage = new AuditViewService(createAuditService({
        retrieveAuditsAsync: async () => [createAudit({})]
    }));

    expect((await withFullPage.retrieveAuditPageViewAsync(0, "")).hasMore).toBe(true);
    expect((await withShortPage.retrieveAuditPageViewAsync(0, "")).hasMore).toBe(false);
});

it("should fall back to a placeholder for the nullable columns", async () => {
    const auditViewService = new AuditViewService(createAuditService({
        retrieveAuditByIdAsync: async () =>
            createAudit({ correlationId: null, fileName: null, logLevel: "" })
    }));

    const auditDetailView = await auditViewService
        .retrieveAuditDetailViewAsync("1b0f6a2e-4c31-4a8f-9a1d-77c2c0d51000");

    expect(auditDetailView.correlationIdText).toBe("—");
    expect(auditDetailView.fileNameText).toBe("—");
    expect(auditDetailView.logLevelText).toBe("Information");
});

it("should wrap a foundation service failure in a view service exception", async () => {
    const auditViewService = new AuditViewService(createAuditService({
        retrieveAuditsAsync: async () => { throw new Error("dependency down"); }
    }));

    await expect(auditViewService.retrieveAuditPageViewAsync(0, ""))
        .rejects.toThrowError("We could not load the audits, please try again or contact support.");
});
