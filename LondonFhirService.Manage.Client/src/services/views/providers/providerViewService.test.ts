import { expect, it } from "vitest";
import { ProviderViewService } from "./providerViewService";
import type { IProviderService } from "../../foundations/providers/iProviderService";
import type { Provider } from "../../../models/foundations/providers/Provider";

const createProvider = (overrides: Partial<Provider>): Provider => ({
    id: "8f4b2c26-0c0d-4a0e-9f2b-2a2f4a6c1111",
    friendlyName: "Discovery Data Service",
    fullyQualifiedName: "https://dds.example.nhs.uk/STU3",
    fhirVersion: "STU3",
    isActive: true,
    activeFrom: null,
    activeTo: null,
    isForComparisonOnly: false,
    isPrimary: false,
    createdBy: "seed",
    createdDate: "2026-01-05T09:30:00+00:00",
    updatedBy: "seed",
    updatedDate: "2026-01-05T09:30:00+00:00",
    ...overrides
});

const createProviderService = (providers: Provider[]): IProviderService => ({
    retrieveAllProvidersAsync: async () => providers,
    retrieveProviderByIdAsync: async providerId =>
        providers.find(provider => provider.id === providerId) as Provider
});

it("should map an open ended active provider to an active view", async () => {
    const providerViewService =
        new ProviderViewService(createProviderService([createProvider({ isPrimary: true })]));

    const [providerListItemView] =
        await providerViewService.retrieveProviderListItemViewsAsync();

    expect(providerListItemView.statusText).toBe("Active");
    expect(providerListItemView.statusClassName).toBe("badge bg-success");
    expect(providerListItemView.roleText).toBe("Primary");
    expect(providerListItemView.activePeriodText).toBe("Always");
    expect(providerListItemView.detailUrl)
        .toBe("/providers/8f4b2c26-0c0d-4a0e-9f2b-2a2f4a6c1111");
});

it("should map windows outside of today to scheduled and expired views", async () => {
    const providerViewService = new ProviderViewService(createProviderService([
        createProvider({ id: "scheduled", friendlyName: "A", activeFrom: "2999-01-01T00:00:00+00:00" }),
        createProvider({ id: "expired", friendlyName: "B", activeTo: "2000-01-01T00:00:00+00:00" }),
        createProvider({ id: "inactive", friendlyName: "C", isActive: false })
    ]));

    const providerListItemViews =
        await providerViewService.retrieveProviderListItemViewsAsync();

    expect(providerListItemViews.map(view => view.statusText))
        .toEqual(["Scheduled", "Expired", "Inactive"]);
});

it("should sort the views by friendly name", async () => {
    const providerViewService = new ProviderViewService(createProviderService([
        createProvider({ id: "second", friendlyName: "Zebra" }),
        createProvider({ id: "first", friendlyName: "Aardvark" })
    ]));

    const providerListItemViews =
        await providerViewService.retrieveProviderListItemViewsAsync();

    expect(providerListItemViews.map(view => view.friendlyName))
        .toEqual(["Aardvark", "Zebra"]);
});

it("should filter the views on any searchable column, ignoring case", async () => {
    const providerViewService = new ProviderViewService(createProviderService([]));

    const providerListItemViews = [
        createProvider({ id: "one", friendlyName: "Discovery", fhirVersion: "STU3" }),
        createProvider({ id: "two", friendlyName: "Barts", fhirVersion: "R4" })
    ].map(provider => ({
        id: provider.id,
        friendlyName: provider.friendlyName,
        fullyQualifiedName: provider.fullyQualifiedName,
        fhirVersionText: provider.fhirVersion,
        statusText: "Active",
        statusClassName: "badge bg-success",
        roleText: "Secondary",
        roleClassName: "badge bg-light text-dark border",
        activePeriodText: "Always",
        detailUrl: `/providers/${provider.id}`,
        searchableText: `${provider.friendlyName} ${provider.fhirVersion}`.toLowerCase()
    }));

    expect(providerViewService
        .filterProviderListItemViews(providerListItemViews, "  r4 ")
        .map(view => view.id))
        .toEqual(["two"]);

    expect(providerViewService
        .filterProviderListItemViews(providerListItemViews, "")
        .map(view => view.id))
        .toEqual(["one", "two"]);
});

it("should wrap a foundation service failure in a view service exception", async () => {
    const providerViewService = new ProviderViewService({
        retrieveAllProvidersAsync: async () => { throw new Error("dependency down"); },
        retrieveProviderByIdAsync: async () => { throw new Error("dependency down"); }
    });

    await expect(providerViewService.retrieveProviderListItemViewsAsync())
        .rejects.toThrowError("We could not load the providers, please try again or contact support.");
});
