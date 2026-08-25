import moment from "moment";
import { ProviderService } from "../../foundations/providers/providerService";
import { ProviderViewServiceException } from "../../../models/views/providers/exceptions/ProviderViewServiceException";
import type { IProviderService } from "../../foundations/providers/iProviderService";
import type { IProviderViewService } from "./iProviderViewService";
import type { Provider } from "../../../models/foundations/providers/Provider";
import type { ProviderDetailView } from "../../../models/views/providers/ProviderDetailView";
import type { ProviderListItemView } from "../../../models/views/providers/ProviderListItemView";

const notSetText = "—";
const dateDisplayFormat = "DD MMM YYYY HH:mm";

export class ProviderViewService implements IProviderViewService {
    private readonly providerService: IProviderService;

    constructor(providerService: IProviderService = new ProviderService()) {
        this.providerService = providerService;
    }

    public async retrieveProviderListItemViewsAsync(
        abortSignal?: AbortSignal)
        : Promise<ProviderListItemView[]> {
        try {
            const providers = await this.providerService.retrieveAllProvidersAsync(abortSignal);

            return providers
                .map(provider => this.toProviderListItemView(provider))
                .sort((leftProvider, rightProvider) =>
                    leftProvider.friendlyName.localeCompare(rightProvider.friendlyName));
        } catch (exception) {
            throw new ProviderViewServiceException(
                "We could not load the providers, please try again or contact support.",
                exception);
        }
    }

    public async retrieveProviderDetailViewAsync(
        providerId: string,
        abortSignal?: AbortSignal)
        : Promise<ProviderDetailView> {
        try {
            const provider =
                await this.providerService.retrieveProviderByIdAsync(providerId, abortSignal);

            return this.toProviderDetailView(provider);
        } catch (exception) {
            throw new ProviderViewServiceException(
                "We could not load this provider, please try again or contact support.",
                exception);
        }
    }

    // Presentation filtering over rows that have already been retrieved. The provider registry is
    // a small configuration table, so the master list searches in the browser rather than paying a
    // round trip per keystroke.
    public filterProviderListItemViews(
        providerListItemViews: ProviderListItemView[],
        searchTerm: string)
        : ProviderListItemView[] {
        const trimmedSearchTerm = searchTerm.trim().toLowerCase();

        if (trimmedSearchTerm.length === 0) {
            return providerListItemViews;
        }

        return providerListItemViews.filter(providerListItemView =>
            providerListItemView.searchableText.includes(trimmedSearchTerm));
    }

    private toProviderListItemView(provider: Provider): ProviderListItemView {
        const statusText = this.mapStatusToDisplayText(provider);
        const roleText = this.mapRoleToDisplayText(provider);

        return {
            id: provider.id,
            friendlyName: provider.friendlyName || notSetText,
            fullyQualifiedName: provider.fullyQualifiedName || notSetText,
            fhirVersionText: provider.fhirVersion || notSetText,
            statusText: statusText,
            statusClassName: this.mapStatusToClassName(statusText),
            roleText: roleText,
            roleClassName: this.mapRoleToClassName(roleText),
            activePeriodText: this.formatActivePeriod(provider),
            detailUrl: this.buildDetailUrl(provider.id),
            searchableText: [
                provider.friendlyName,
                provider.fullyQualifiedName,
                provider.fhirVersion,
                statusText,
                roleText
            ].join(" ").toLowerCase()
        };
    }

    private toProviderDetailView(provider: Provider): ProviderDetailView {
        const statusText = this.mapStatusToDisplayText(provider);
        const roleText = this.mapRoleToDisplayText(provider);

        return {
            id: provider.id,
            friendlyName: provider.friendlyName || notSetText,
            fullyQualifiedName: provider.fullyQualifiedName || notSetText,
            fhirVersionText: provider.fhirVersion || notSetText,
            statusText: statusText,
            statusClassName: this.mapStatusToClassName(statusText),
            roleText: roleText,
            roleClassName: this.mapRoleToClassName(roleText),
            isPrimaryText: this.mapBooleanToDisplayText(provider.isPrimary),
            isForComparisonOnlyText: this.mapBooleanToDisplayText(provider.isForComparisonOnly),
            activeFromText: this.formatDate(provider.activeFrom),
            activeToText: this.formatDate(provider.activeTo),
            activePeriodText: this.formatActivePeriod(provider),
            createdBy: provider.createdBy || notSetText,
            createdDateText: this.formatDate(provider.createdDate),
            updatedBy: provider.updatedBy || notSetText,
            updatedDateText: this.formatDate(provider.updatedDate),
            detailUrl: this.buildDetailUrl(provider.id)
        };
    }

    private mapStatusToDisplayText(provider: Provider): string {
        if (provider.isActive === false) {
            return "Inactive";
        }

        const now = moment();

        if (provider.activeFrom !== null && now.isBefore(moment(provider.activeFrom))) {
            return "Scheduled";
        }

        if (provider.activeTo !== null && now.isAfter(moment(provider.activeTo))) {
            return "Expired";
        }

        return "Active";
    }

    private mapStatusToClassName(statusText: string): string {
        const statusClassNames: Record<string, string> = {
            "Active": "badge bg-success",
            "Scheduled": "badge bg-info text-dark",
            "Expired": "badge bg-warning text-dark",
            "Inactive": "badge bg-secondary"
        };

        return statusClassNames[statusText] ?? "badge bg-secondary";
    }

    private mapRoleToDisplayText(provider: Provider): string {
        if (provider.isPrimary) {
            return "Primary";
        }

        if (provider.isForComparisonOnly) {
            return "Comparison only";
        }

        return "Secondary";
    }

    private mapRoleToClassName(roleText: string): string {
        const roleClassNames: Record<string, string> = {
            "Primary": "badge bg-primary",
            "Comparison only": "badge bg-info text-dark",
            "Secondary": "badge bg-light text-dark border"
        };

        return roleClassNames[roleText] ?? "badge bg-light text-dark border";
    }

    private mapBooleanToDisplayText(value: boolean): string {
        return value ? "Yes" : "No";
    }

    private formatActivePeriod(provider: Provider): string {
        if (provider.activeFrom === null && provider.activeTo === null) {
            return "Always";
        }

        return `${this.formatDate(provider.activeFrom)} to ${this.formatDate(provider.activeTo)}`;
    }

    private formatDate(value: string | null): string {
        if (value === null || value.length === 0) {
            return notSetText;
        }

        const parsedValue = moment(value);

        return parsedValue.isValid() ? parsedValue.format(dateDisplayFormat) : notSetText;
    }

    private buildDetailUrl(providerId: string): string {
        return `/providers/${encodeURIComponent(providerId)}`;
    }
}
