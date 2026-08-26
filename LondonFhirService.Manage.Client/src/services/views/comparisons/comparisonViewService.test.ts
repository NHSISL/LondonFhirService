import { expect, it } from "vitest";
import { ComparisonViewService, comparisonPageSize } from "./comparisonViewService";
import { fhirRecordStatuses } from "../../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecord } from "../../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecordDifference } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifference";
import type { IFhirRecordDifferenceService } from "../../foundations/fhirRecordDifferences/iFhirRecordDifferenceService";
import type { IFhirRecordService } from "../../foundations/fhirRecords/iFhirRecordService";

const diffJson = JSON.stringify({
    correlationId: "abc-123",
    diffCount: 3,
    diffs: [
        { type: "modified", path: "name[0].family", oldValue: "Smith", newValue: "Smyth" },
        { type: "added", path: "telecom[1]", newValue: "email" },
        { type: "modified", path: "gender", oldValue: "male", newValue: "female" }
    ]
});

const createFhirRecordDifference = (
    overrides: Partial<FhirRecordDifference> = {})
    : FhirRecordDifference => ({
    id: "1f1b0a3c-2c31-4a5f-9d0e-6c6a1b2c3d4e",
    primaryId: "aaaaaaaa-0000-0000-0000-000000000001",
    secondaryId: "bbbbbbbb-0000-0000-0000-000000000002",
    correlationId: "abc-123",
    diffJson: diffJson,
    diffCount: 3,
    acceptableDiffCount: 1,
    comparedAt: "2026-05-04T09:30:00+00:00",
    comment: null,
    isResolved: false,
    createdBy: "compare-queue",
    createdDate: "2026-05-04T09:30:00+00:00",
    updatedBy: "compare-queue",
    updatedDate: "2026-05-04T09:30:00+00:00",
    ...overrides
});

const bundleFor = (family: string) => JSON.stringify({
    resourceType: "Bundle",
    entry: [
        {
            resource: {
                resourceType: "Patient",
                id: "patient-1",
                name: [{ use: "official", given: ["Alex"], family: family }]
            }
        }
    ]
});

const createFhirRecord = (overrides: Partial<FhirRecord> = {}): FhirRecord => ({
    id: "aaaaaaaa-0000-0000-0000-000000000001",
    correlationId: "abc-123",
    jsonPayload: bundleFor("Smith"),
    sourceName: "Discovery Data Service",
    isPrimarySource: true,
    isProcessed: true,
    status: fhirRecordStatuses.completed,
    insertedDate: "2026-05-04T09:29:00+00:00",
    createdBy: "ingest",
    createdDate: "2026-05-04T09:29:00+00:00",
    updatedBy: "ingest",
    updatedDate: "2026-05-04T09:29:00+00:00",
    ...overrides
});

// One place to stub each service, so adding a member to an interface does not ripple through
// every test that only cares about one call.
const createFhirRecordDifferenceService = (
    overrides: Partial<IFhirRecordDifferenceService> = {})
    : IFhirRecordDifferenceService => ({
    retrieveFhirRecordDifferencesAsync: async () => [],
    retrieveFhirRecordDifferenceByIdAsync: async () => createFhirRecordDifference(),
    modifyFhirRecordDifferenceAsync: async fhirRecordDifference => fhirRecordDifference,
    ...overrides
});

const createFhirRecordService = (
    overrides: Partial<IFhirRecordService> = {})
    : IFhirRecordService => ({
    retrieveFhirRecordByIdAsync: async fhirRecordId => createFhirRecord({ id: fhirRecordId }),
    ...overrides
});

const rejects = async (): Promise<never> => {
    throw new Error("dependency down");
};

it("should break a comparison down by difference kind", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferencesAsync: async () => [createFhirRecordDifference()]
        }),
        createFhirRecordService());

    const { comparisons } =
        await comparisonViewService.retrieveComparisonPageViewAsync(0, "", false);

    expect(comparisons[0].breakdownText).toBe("2 modified, 1 added");
    expect(comparisons[0].diffCountText).toBe("3 differences");
    expect(comparisons[0].diffCountClassName).toBe("badge bg-danger");
    expect(comparisons[0].resolutionText).toBe("Open");
    expect(comparisons[0].detailUrl)
        .toBe("/admin/comparisons/1f1b0a3c-2c31-4a5f-9d0e-6c6a1b2c3d4e");
});

it("should read a comparison with no differences as a match", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferencesAsync: async () => [
                createFhirRecordDifference({
                    diffCount: 0,
                    acceptableDiffCount: 0,
                    isResolved: true,

                    diffJson: JSON.stringify({
                        correlationId: "abc-123",
                        diffCount: 0,
                        diffs: []
                    })
                })
            ]
        }),
        createFhirRecordService());

    const { comparisons } =
        await comparisonViewService.retrieveComparisonPageViewAsync(0, "", false);

    expect(comparisons[0].diffCountText).toBe("0 differences");
    expect(comparisons[0].diffCountClassName).toBe("badge bg-success");
    expect(comparisons[0].resolutionText).toBe("Resolved");
    expect(comparisons[0].breakdownText).toBe("—");
});

// DiffJson is text in a column. A row written by an older shape of the engine, or truncated, must
// not take the whole list down with it.
it("should still list a comparison whose stored result cannot be read", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferencesAsync: async () => [
                createFhirRecordDifference({ diffJson: "{ not json" })
            ]
        }),
        createFhirRecordService());

    const { comparisons } =
        await comparisonViewService.retrieveComparisonPageViewAsync(0, "", false);

    expect(comparisons).toHaveLength(1);
    expect(comparisons[0].breakdownText).toBe("—");
});

// The endpoint reports no total, so a full page is the only signal that another may follow.
it("should report more pages only when the page came back full", async () => {
    const fullPage = Array.from(
        { length: comparisonPageSize },
        (_unused, index) => createFhirRecordDifference({ id: `comparison-${index}` }));

    const withFullPage = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferencesAsync: async () => fullPage
        }),
        createFhirRecordService());

    const withShortPage = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferencesAsync: async () => [createFhirRecordDifference()]
        }),
        createFhirRecordService());

    expect((await withFullPage.retrieveComparisonPageViewAsync(0, "", false)).hasMore).toBe(true);
    expect((await withShortPage.retrieveComparisonPageViewAsync(0, "", false)).hasMore).toBe(false);
});

it("should ask for the page the caller wanted", async () => {
    let requestedSkip = -1;

    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferencesAsync: async query => {
                requestedSkip = query.skip;

                return [];
            }
        }),
        createFhirRecordService());

    await comparisonViewService.retrieveComparisonPageViewAsync(2, "", false);

    expect(requestedSkip).toBe(2 * comparisonPageSize);
});

it("should parse both sides of a comparison and label their roles", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService(),
        createFhirRecordService({
            retrieveFhirRecordByIdAsync: async fhirRecordId =>
                createFhirRecord({
                    id: fhirRecordId,
                    sourceName: fhirRecordId.startsWith("aaaa") ? "Primary DS" : "Secondary DS",
                    jsonPayload: bundleFor(fhirRecordId.startsWith("aaaa") ? "Smith" : "Smyth")
                })
        }));

    const comparison = await comparisonViewService.retrieveComparisonDetailViewAsync("any-id");

    expect(comparison.primarySource?.sourceName).toBe("Primary DS");
    expect(comparison.primarySource?.roleText).toBe("Primary");
    expect(comparison.primarySource?.bundle.patient.nameFamily).toBe("Smith");
    expect(comparison.secondarySource?.sourceName).toBe("Secondary DS");
    expect(comparison.secondarySource?.roleText).toBe("Secondary");
    expect(comparison.secondarySource?.bundle.patient.nameFamily).toBe("Smyth");
    expect(comparison.sourcesError).toBeNull();
    expect(comparison.outstandingDiffCountText).toBe("2");
});

// A record can be removed while its comparison is still on the shelf. The differences the engine
// recorded are still worth seeing, so a missing side is reported rather than thrown.
it("should render the sides it has when one record cannot be loaded", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService(),
        createFhirRecordService({
            retrieveFhirRecordByIdAsync: async fhirRecordId =>
                fhirRecordId.startsWith("bbbb")
                    ? await rejects()
                    : createFhirRecord({ id: fhirRecordId })
        }));

    const comparison = await comparisonViewService.retrieveComparisonDetailViewAsync("any-id");

    expect(comparison.primarySource).not.toBeNull();
    expect(comparison.secondarySource).toBeNull();
    expect(comparison.sourcesError).toContain("secondary");
    expect(comparison.diffs).toHaveLength(3);
});

it("should give every difference a key that survives a repeated path", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferenceByIdAsync: async () => createFhirRecordDifference({
                diffJson: JSON.stringify({
                    correlationId: "abc-123",
                    diffCount: 2,
                    diffs: [
                        { type: "removed", path: "telecom[0]", oldValue: "one" },
                        { type: "removed", path: "telecom[0]", oldValue: "two" }
                    ]
                })
            })
        }),
        createFhirRecordService());

    const comparison = await comparisonViewService.retrieveComparisonDetailViewAsync("any-id");
    const keys = comparison.diffs.map(diff => diff.key);

    expect(new Set(keys).size).toBe(2);
});

// The server rejects a modify that does not carry back the created audit values it holds, and
// DiffJson is not the operator's to edit, so the record is re-read and only the review fields
// are replaced.
it("should carry the untouched record back when saving the review fields", async () => {
    let modifiedFhirRecordDifference: FhirRecordDifference | null = null;

    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({
            modifyFhirRecordDifferenceAsync: async fhirRecordDifference => {
                modifiedFhirRecordDifference = fhirRecordDifference;

                return fhirRecordDifference;
            }
        }),
        createFhirRecordService());

    await comparisonViewService.updateComparisonAsync("any-id", {
        comment: "  Known dosage rounding difference  ",
        isResolved: true,
        acceptableDiffCount: "3"
    });

    const saved = modifiedFhirRecordDifference as FhirRecordDifference | null;

    expect(saved?.comment).toBe("Known dosage rounding difference");
    expect(saved?.isResolved).toBe(true);
    expect(saved?.acceptableDiffCount).toBe(3);
    expect(saved?.diffJson).toBe(diffJson);
    expect(saved?.createdBy).toBe("compare-queue");
    expect(saved?.createdDate).toBe("2026-05-04T09:30:00+00:00");
});

it("should clear a comment that was blanked out", async () => {
    let modifiedFhirRecordDifference: FhirRecordDifference | null = null;

    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({
            retrieveFhirRecordDifferenceByIdAsync: async () =>
                createFhirRecordDifference({ comment: "old note" }),

            modifyFhirRecordDifferenceAsync: async fhirRecordDifference => {
                modifiedFhirRecordDifference = fhirRecordDifference;

                return fhirRecordDifference;
            }
        }),
        createFhirRecordService());

    await comparisonViewService.updateComparisonAsync("any-id", {
        comment: "   ",
        isResolved: false,
        acceptableDiffCount: "0"
    });

    expect((modifiedFhirRecordDifference as FhirRecordDifference | null)?.comment).toBeNull();
});

it("should surface a view service exception when the list cannot be loaded", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({ retrieveFhirRecordDifferencesAsync: rejects }),
        createFhirRecordService());

    await expect(comparisonViewService.retrieveComparisonPageViewAsync(0, "", false))
        .rejects.toThrow("We could not load the comparisons");
});

it("should surface a view service exception when the comparison cannot be loaded", async () => {
    const comparisonViewService = new ComparisonViewService(
        createFhirRecordDifferenceService({ retrieveFhirRecordDifferenceByIdAsync: rejects }),
        createFhirRecordService());

    await expect(comparisonViewService.retrieveComparisonDetailViewAsync("any-id"))
        .rejects.toThrow("We could not load this comparison");
});
