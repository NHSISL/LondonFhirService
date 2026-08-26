import moment from "moment";
import { ComparisonViewServiceException } from "../../../models/views/comparisons/exceptions/ComparisonViewServiceException";
import { FhirRecordDifferenceService } from "../../foundations/fhirRecordDifferences/fhirRecordDifferenceService";
import { FhirRecordService } from "../../foundations/fhirRecords/fhirRecordService";
import { fhirRecordStatuses } from "../../../models/foundations/fhirRecords/FhirRecord";
import { parseBundle } from "../../../helpers/fhir/fhirBundleParser";
import type { ComparisonDetailView } from "../../../models/views/comparisons/ComparisonDetailView";
import type { ComparisonFormValues } from "../../../models/views/comparisons/ComparisonFormValues";
import type { ComparisonListItemView } from "../../../models/views/comparisons/ComparisonListItemView";
import type { ComparisonPageView } from "../../../models/views/comparisons/ComparisonPageView";
import type { ComparisonResult } from "../../../models/foundations/comparisons/ComparisonResult";
import type { ComparisonSourceView } from "../../../models/views/comparisons/ComparisonSourceView";
import type { DiffItem } from "../../../models/foundations/comparisons/DiffItem";
import type { DiffItemView } from "../../../models/views/comparisons/DiffItemView";
import type { FhirRecord } from "../../../models/foundations/fhirRecords/FhirRecord";
import type { FhirRecordDifference } from "../../../models/foundations/fhirRecordDifferences/FhirRecordDifference";
import type { IComparisonViewService } from "./iComparisonViewService";
import type { IFhirRecordDifferenceService } from "../../foundations/fhirRecordDifferences/iFhirRecordDifferenceService";
import type { IFhirRecordService } from "../../foundations/fhirRecords/iFhirRecordService";

const notSetText = "—";
const dateDisplayFormat = "DD MMM YYYY HH:mm:ss";

// Deliberately smaller than the audits and metrics lists. Every row carries its whole DiffJson,
// because this endpoint cannot project it away - see fhirRecordDifferenceApiBroker.queries - and a
// correlation whose bundles disagree badly can push that into the hundreds of kilobytes. Twenty
// five rows keeps a page bounded while still filling the screen; infinite scroll fetches the rest
// on demand. Also stays under the server's own EnableQuery page size, which would otherwise
// truncate a larger ask without saying so.
export const comparisonPageSize = 25;

// Ordered so the summary reads the way an operator triages: what changed, then what only one side
// has, then what the engine could not decide on its own.
const diffTypeOrder = [
    "modified",
    "added",
    "removed",
    "entry-count-mismatch",
    "manual-review-required"
];

const diffTypeTexts: Record<string, string> = {
    "modified": "Modified",
    "added": "Added",
    "removed": "Removed",
    "entry-count-mismatch": "Entry count mismatch",
    "manual-review-required": "Manual review required"
};

const diffTypeClassNames: Record<string, string> = {
    "modified": "badge bg-warning text-dark",
    "added": "badge bg-success",
    "removed": "badge bg-danger",
    "entry-count-mismatch": "badge bg-warning text-dark",
    "manual-review-required": "badge bg-secondary"
};

const fhirRecordStatusTexts: Record<number, string> = {
    [fhirRecordStatuses.pending]: "Pending",
    [fhirRecordStatuses.processing]: "Processing",
    [fhirRecordStatuses.completed]: "Completed",
    [fhirRecordStatuses.failed]: "Failed"
};

const fhirRecordStatusClassNames: Record<number, string> = {
    [fhirRecordStatuses.pending]: "badge bg-secondary",
    [fhirRecordStatuses.processing]: "badge bg-info text-dark",
    [fhirRecordStatuses.completed]: "badge bg-success",
    [fhirRecordStatuses.failed]: "badge bg-danger"
};

export class ComparisonViewService implements IComparisonViewService {
    private readonly fhirRecordDifferenceService: IFhirRecordDifferenceService;
    private readonly fhirRecordService: IFhirRecordService;

    constructor(
        fhirRecordDifferenceService: IFhirRecordDifferenceService =
        new FhirRecordDifferenceService(),
        fhirRecordService: IFhirRecordService = new FhirRecordService()) {
        this.fhirRecordDifferenceService = fhirRecordDifferenceService;
        this.fhirRecordService = fhirRecordService;
    }

    public async retrieveComparisonPageViewAsync(
        pageNumber: number,
        searchTerm: string,
        unresolvedOnly: boolean,
        abortSignal?: AbortSignal)
        : Promise<ComparisonPageView> {
        try {
            const fhirRecordDifferences =
                await this.fhirRecordDifferenceService.retrieveFhirRecordDifferencesAsync(
                    {
                        skip: pageNumber * comparisonPageSize,
                        take: comparisonPageSize,
                        searchTerm: searchTerm,
                        unresolvedOnly: unresolvedOnly
                    },
                    abortSignal);

            return {
                comparisons: fhirRecordDifferences.map(fhirRecordDifference =>
                    this.toComparisonListItemView(fhirRecordDifference)),

                // The endpoint does not report a total, so a full page is taken as a signal that
                // there may be another one. A short page is the end.
                hasMore: fhirRecordDifferences.length === comparisonPageSize
            };
        } catch (exception) {
            throw new ComparisonViewServiceException(
                "We could not load the comparisons, please try again or contact support.",
                exception);
        }
    }

    public async retrieveComparisonDetailViewAsync(
        fhirRecordDifferenceId: string,
        abortSignal?: AbortSignal)
        : Promise<ComparisonDetailView> {
        try {
            const fhirRecordDifference =
                await this.fhirRecordDifferenceService.retrieveFhirRecordDifferenceByIdAsync(
                    fhirRecordDifferenceId,
                    abortSignal);

            return await this.toComparisonDetailViewAsync(fhirRecordDifference, abortSignal);
        } catch (exception) {
            throw new ComparisonViewServiceException(
                "We could not load this comparison, please try again or contact support.",
                exception);
        }
    }

    public createComparisonFormValues(): ComparisonFormValues {
        return {
            comment: "",
            isResolved: false,
            acceptableDiffCount: "0"
        };
    }

    // The current record is re-read rather than reconstructed from the page, because the server
    // compares CreatedBy and CreatedDate against storage and rejects a modify that does not carry
    // them back unchanged - and because DiffJson, which the operator never edits, has to travel
    // back with the rest of the record.
    public async updateComparisonAsync(
        fhirRecordDifferenceId: string,
        comparisonFormValues: ComparisonFormValues)
        : Promise<ComparisonDetailView> {
        try {
            const currentFhirRecordDifference =
                await this.fhirRecordDifferenceService.retrieveFhirRecordDifferenceByIdAsync(
                    fhirRecordDifferenceId);

            const modifiedFhirRecordDifference =
                await this.fhirRecordDifferenceService.modifyFhirRecordDifferenceAsync({
                    ...currentFhirRecordDifference,
                    comment: this.toNullableText(comparisonFormValues.comment),
                    isResolved: comparisonFormValues.isResolved,

                    acceptableDiffCount:
                        this.toAcceptableDiffCount(comparisonFormValues.acceptableDiffCount)
                });

            return await this.toComparisonDetailViewAsync(modifiedFhirRecordDifference);
        } catch (exception) {
            throw new ComparisonViewServiceException(
                "We could not save this comparison, please correct any errors and try again.",
                exception);
        }
    }

    // Both records are fetched together, and a side that cannot be read is reported rather than
    // thrown: a comparison whose secondary record has since been removed is still worth seeing.
    private async toComparisonDetailViewAsync(
        fhirRecordDifference: FhirRecordDifference,
        abortSignal?: AbortSignal)
        : Promise<ComparisonDetailView> {
        const [primaryResult, secondaryResult] = await Promise.allSettled([
            this.fhirRecordService.retrieveFhirRecordByIdAsync(
                fhirRecordDifference.primaryId,
                abortSignal),

            this.fhirRecordService.retrieveFhirRecordByIdAsync(
                fhirRecordDifference.secondaryId,
                abortSignal)
        ]);

        // allSettled swallows a cancellation as well as a failure, and a cancelled fetch must not
        // be cached as a comparison whose records are both missing. Re-raise it so the query is
        // discarded, the way a rejecting fetch would be.
        if (abortSignal?.aborted === true) {
            throw abortSignal.reason;
        }

        const diffs = this.readDiffs(fhirRecordDifference.diffJson);
        const outstandingDiffCount =
            fhirRecordDifference.diffCount - fhirRecordDifference.acceptableDiffCount;

        return {
            id: fhirRecordDifference.id,
            correlationId: fhirRecordDifference.correlationId || notSetText,
            diffCount: fhirRecordDifference.diffCount,
            diffCountText: this.formatDiffCount(fhirRecordDifference.diffCount),
            diffCountClassName: this.mapDiffCountToClassName(fhirRecordDifference.diffCount),

            acceptableDiffCountText:
                String(Math.max(fhirRecordDifference.acceptableDiffCount, 0)),

            outstandingDiffCountText: String(Math.max(outstandingDiffCount, 0)),
            breakdownText: this.formatBreakdown(diffs),
            comparedAtText: this.formatDate(fhirRecordDifference.comparedAt),
            resolutionText: this.mapResolutionToDisplayText(fhirRecordDifference.isResolved),

            resolutionClassName:
                this.mapResolutionToClassName(fhirRecordDifference.isResolved),

            commentText: fhirRecordDifference.comment ?? notSetText,
            updatedByText: fhirRecordDifference.updatedBy || notSetText,
            updatedDateText: this.formatDate(fhirRecordDifference.updatedDate),
            diffs: diffs.map((diff, index) => this.toDiffItemView(diff, index)),
            primarySource: this.toComparisonSourceView(primaryResult, "primary"),
            secondarySource: this.toComparisonSourceView(secondaryResult, "secondary"),
            sourcesError: this.describeSourcesError(primaryResult, secondaryResult),
            editValues: this.toComparisonFormValues(fhirRecordDifference)
        };
    }

    private toComparisonListItemView(
        fhirRecordDifference: FhirRecordDifference)
        : ComparisonListItemView {
        return {
            id: fhirRecordDifference.id,
            correlationId: fhirRecordDifference.correlationId || notSetText,
            diffCountText: this.formatDiffCount(fhirRecordDifference.diffCount),
            diffCountClassName: this.mapDiffCountToClassName(fhirRecordDifference.diffCount),

            acceptableDiffCountText:
                String(Math.max(fhirRecordDifference.acceptableDiffCount, 0)),

            breakdownText: this.formatBreakdown(this.readDiffs(fhirRecordDifference.diffJson)),
            comparedAtText: this.formatDate(fhirRecordDifference.comparedAt),
            resolutionText: this.mapResolutionToDisplayText(fhirRecordDifference.isResolved),

            resolutionClassName:
                this.mapResolutionToClassName(fhirRecordDifference.isResolved),

            commentText: fhirRecordDifference.comment ?? notSetText,
            detailUrl: this.buildDetailUrl(fhirRecordDifference.id)
        };
    }

    private toComparisonSourceView(
        fhirRecordResult: PromiseSettledResult<FhirRecord>,
        side: "primary" | "secondary")
        : ComparisonSourceView | null {
        if (fhirRecordResult.status !== "fulfilled") {
            return null;
        }

        const fhirRecord = fhirRecordResult.value;

        return {
            sourceName: fhirRecord.sourceName || notSetText,
            roleText: side === "primary" ? "Primary" : "Secondary",

            roleClassName: side === "primary"
                ? "badge bg-primary"
                : "badge bg-light text-dark border",

            statusText: fhirRecordStatusTexts[fhirRecord.status] ?? notSetText,

            statusClassName:
                fhirRecordStatusClassNames[fhirRecord.status] ?? "badge bg-secondary",

            createdDateText: this.formatDate(fhirRecord.createdDate),
            formattedJsonPayload: this.formatJson(fhirRecord.jsonPayload),
            bundle: parseBundle(fhirRecord.jsonPayload)
        };
    }

    private describeSourcesError(
        primaryResult: PromiseSettledResult<FhirRecord>,
        secondaryResult: PromiseSettledResult<FhirRecord>)
        : string | null {
        const unreadableSides = [
            primaryResult.status === "rejected" ? "primary" : null,
            secondaryResult.status === "rejected" ? "secondary" : null
        ].filter((side): side is string => side !== null);

        if (unreadableSides.length === 0) {
            return null;
        }

        return `The ${unreadableSides.join(" and ")} record for this comparison could not be ` +
            "loaded. The differences below are the ones that were recorded when the comparison " +
            "ran.";
    }

    private toComparisonFormValues(
        fhirRecordDifference: FhirRecordDifference)
        : ComparisonFormValues {
        return {
            comment: fhirRecordDifference.comment ?? "",
            isResolved: fhirRecordDifference.isResolved,
            acceptableDiffCount: String(Math.max(fhirRecordDifference.acceptableDiffCount, 0))
        };
    }

    private toDiffItemView(diff: DiffItem, index: number): DiffItemView {
        return {
            // The engine can write the same path more than once for one comparison - an array
            // compared element by element, say - so the index is what keeps the key unique.
            key: `${index}-${diff.path}`,

            type: diff.type,
            typeText: diffTypeTexts[diff.type] ?? diff.type,
            typeClassName: diffTypeClassNames[diff.type] ?? "badge bg-secondary",
            path: diff.path || notSetText,
            oldValueText: diff.oldValue,
            newValueText: diff.newValue,
            resourceTypeText: diff.resourceType,
            identifierText: diff.identifier,
            reasonText: diff.reason
        };
    }

    // DiffJson is written by the comparison coordination service, but it is still text in a
    // column: a row written by an older shape of the engine, or truncated, must not take the whole
    // list down. An unreadable one simply contributes no breakdown.
    private readDiffs(diffJson: string): DiffItem[] {
        if (diffJson.trim().length === 0) {
            return [];
        }

        let parsedDiffJson: unknown;

        try {
            parsedDiffJson = JSON.parse(diffJson);
        } catch {
            return [];
        }

        if (typeof parsedDiffJson !== "object" || parsedDiffJson === null) {
            return [];
        }

        const comparisonResult = parsedDiffJson as Partial<ComparisonResult>;

        if (Array.isArray(comparisonResult.diffs) === false) {
            return [];
        }

        return (comparisonResult.diffs as unknown[])
            .map(rawDiff => this.toDiffItem(rawDiff))
            .filter((diff): diff is DiffItem => diff !== null);
    }

    private toDiffItem(rawDiff: unknown): DiffItem | null {
        if (typeof rawDiff !== "object" || rawDiff === null) {
            return null;
        }

        const source = rawDiff as Record<string, unknown>;

        return {
            type: this.readString(source.type) ?? "",
            path: this.readString(source.path) ?? "",
            oldValue: this.readString(source.oldValue),
            newValue: this.readString(source.newValue),
            resourceType: this.readString(source.resourceType),
            identifier: this.readString(source.identifier),
            reason: this.readString(source.reason)
        };
    }

    // "12 modified, 3 added" - enough to triage a row without opening it.
    private formatBreakdown(diffs: DiffItem[]): string {
        if (diffs.length === 0) {
            return notSetText;
        }

        const countsByType = new Map<string, number>();

        for (const diff of diffs) {
            countsByType.set(diff.type, (countsByType.get(diff.type) ?? 0) + 1);
        }

        const knownTypes = diffTypeOrder.filter(diffType => countsByType.has(diffType));

        const unknownTypes = [...countsByType.keys()]
            .filter(diffType => diffTypeOrder.includes(diffType) === false)
            .sort();

        return [...knownTypes, ...unknownTypes]
            .map(diffType =>
                `${countsByType.get(diffType)} ${(diffTypeTexts[diffType] ?? diffType)
                    .toLowerCase()}`)
            .join(", ");
    }

    private formatDiffCount(diffCount: number): string {
        return diffCount === 1 ? "1 difference" : `${diffCount} differences`;
    }

    private mapDiffCountToClassName(diffCount: number): string {
        return diffCount > 0 ? "badge bg-danger" : "badge bg-success";
    }

    private mapResolutionToDisplayText(isResolved: boolean): string {
        return isResolved ? "Resolved" : "Open";
    }

    private mapResolutionToClassName(isResolved: boolean): string {
        return isResolved ? "badge bg-success" : "badge bg-warning text-dark";
    }

    // Pretty printed once, on the way into the view, so the JSON panels do not re-format a whole
    // bundle on every open and close.
    private formatJson(jsonPayload: string): string {
        if (jsonPayload.trim().length === 0) {
            return "";
        }

        try {
            return JSON.stringify(JSON.parse(jsonPayload), null, 2);
        } catch {
            return jsonPayload;
        }
    }

    private toNullableText(value: string): string | null {
        const trimmedValue = value.trim();

        return trimmedValue.length > 0 ? trimmedValue : null;
    }

    private toAcceptableDiffCount(value: string): number {
        const parsedValue = Number.parseInt(value.trim(), 10);

        return Number.isNaN(parsedValue) || parsedValue < 0 ? 0 : parsedValue;
    }

    private readString(rawValue: unknown): string | null {
        return typeof rawValue === "string" && rawValue.length > 0 ? rawValue : null;
    }

    private formatDate(value: string): string {
        if (!value || value.length === 0) {
            return notSetText;
        }

        const parsedValue = moment(value);

        return parsedValue.isValid() ? parsedValue.format(dateDisplayFormat) : notSetText;
    }

    private buildDetailUrl(fhirRecordDifferenceId: string): string {
        return `/admin/comparisons/${encodeURIComponent(fhirRecordDifferenceId)}`;
    }
}
