import { expect, it } from "vitest";
import { ProviderViewService } from "./providerViewService";
import type { IProviderService } from "../../foundations/providers/iProviderService";
import type { Provider } from "../../../models/foundations/providers/Provider";
import type { ProviderRegistration } from "../../../models/foundations/providers/ProviderRegistration";

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

const rejects = async (): Promise<never> => {
    throw new Error("dependency down");
};

// One place to stub IProviderService, so adding a member to the interface does not ripple through
// every test that only cares about one call.
const createProviderService = (overrides: Partial<IProviderService> = {}): IProviderService => ({
    retrieveAllProvidersAsync: async () => [],
    retrieveProviderByIdAsync: async () => createProvider({}),
    addProviderAsync: async providerRegistration => createProvider({ ...providerRegistration }),
    modifyProviderAsync: async provider => provider,
    removeProviderByIdAsync: async () => createProvider({}),
    ...overrides
});

const withProviders = (providers: Provider[]): IProviderService =>
    createProviderService({
        retrieveAllProvidersAsync: async () => providers,
        retrieveProviderByIdAsync: async providerId =>
            providers.find(provider => provider.id === providerId) as Provider
    });

it("should map an open ended active provider to an active view", async () => {
    const providerViewService =
        new ProviderViewService(withProviders([createProvider({ isPrimary: true })]));

    const [providerListItemView] =
        await providerViewService.retrieveProviderListItemViewsAsync();

    expect(providerListItemView.statusText).toBe("Active");
    expect(providerListItemView.statusClassName).toBe("badge bg-success");
    expect(providerListItemView.roleText).toBe("Primary");
    expect(providerListItemView.activePeriodText).toBe("Always");
    expect(providerListItemView.detailUrl)
        .toBe("/admin/providers/8f4b2c26-0c0d-4a0e-9f2b-2a2f4a6c1111");
});

it("should map windows outside of today to scheduled and expired views", async () => {
    const providerViewService = new ProviderViewService(withProviders([
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
    const providerViewService = new ProviderViewService(withProviders([
        createProvider({ id: "second", friendlyName: "Zebra" }),
        createProvider({ id: "first", friendlyName: "Aardvark" })
    ]));

    const providerListItemViews =
        await providerViewService.retrieveProviderListItemViewsAsync();

    expect(providerListItemViews.map(view => view.friendlyName))
        .toEqual(["Aardvark", "Zebra"]);
});

it("should filter the views on any searchable column, ignoring case", async () => {
    const providerViewService = new ProviderViewService(createProviderService());

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
        detailUrl: `/admin/providers/${provider.id}`,
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
    const providerViewService = new ProviderViewService(
        createProviderService({ retrieveAllProvidersAsync: rejects }));

    await expect(providerViewService.retrieveProviderListItemViewsAsync())
        .rejects.toThrowError("We could not load the providers, please try again or contact support.");
});

it("should seed the edit form from the provider, as datetime-local strings", async () => {
    const providerViewService = new ProviderViewService(withProviders([
        createProvider({ activeFrom: "2025-12-24T13:00:00+00:00", isPrimary: true })
    ]));

    const providerDetailView = await providerViewService
        .retrieveProviderDetailViewAsync("8f4b2c26-0c0d-4a0e-9f2b-2a2f4a6c1111");

    expect(providerDetailView.editValues.friendlyName).toBe("Discovery Data Service");
    expect(providerDetailView.editValues.activeFrom).toMatch(/^2025-12-24T\d{2}:00$/);
    expect(providerDetailView.editValues.activeTo).toBe("");
    expect(providerDetailView.editValues.isPrimary).toBe(true);
});

it("should map form values onto a registration with a minted id and iso dates", async () => {
    let captured: ProviderRegistration | null = null;

    const providerViewService = new ProviderViewService(createProviderService({
        addProviderAsync: async providerRegistration => {
            captured = providerRegistration;

            return createProvider({ ...providerRegistration });
        }
    }));

    const providerDetailView = await providerViewService.addProviderAsync({
        friendlyName: "  Barts  ",
        fullyQualifiedName: "  https://barts.example.nhs.uk/STU3  ",
        fhirVersion: " STU3 ",
        isActive: true,
        activeFrom: "2026-09-01T09:00",
        activeTo: "",
        isPrimary: true,
        isForComparisonOnly: false
    });

    const registration = captured as unknown as ProviderRegistration;
    expect(registration.friendlyName).toBe("Barts");
    expect(registration.fullyQualifiedName).toBe("https://barts.example.nhs.uk/STU3");
    expect(registration.fhirVersion).toBe("STU3");
    expect(registration.id.length).toBe(36);
    expect(registration.activeFrom).toMatch(/^2026-09-01T09:00:00/);
    expect(registration.activeTo).toBeNull();
    expect(providerDetailView.roleText).toBe("Primary");
});

it("should carry the stored audit values back unchanged when updating", async () => {
    let captured: Provider | null = null;

    const storedProvider = createProvider({
        createdBy: "original-author",
        createdDate: "2025-03-04T08:15:00+00:00"
    });

    const providerViewService = new ProviderViewService(createProviderService({
        retrieveProviderByIdAsync: async () => storedProvider,
        modifyProviderAsync: async provider => {
            captured = provider;

            return provider;
        }
    }));

    await providerViewService.updateProviderAsync(storedProvider.id, {
        friendlyName: "Renamed",
        fullyQualifiedName: "https://renamed.example.nhs.uk/STU3",
        fhirVersion: "R4",
        isActive: false,
        activeFrom: "",
        activeTo: "",
        isPrimary: false,
        isForComparisonOnly: true
    });

    const modified = captured as unknown as Provider;
    expect(modified.id).toBe(storedProvider.id);
    expect(modified.createdBy).toBe("original-author");
    expect(modified.createdDate).toBe("2025-03-04T08:15:00+00:00");
    expect(modified.friendlyName).toBe("Renamed");
    expect(modified.fhirVersion).toBe("R4");
    expect(modified.isActive).toBe(false);
    expect(modified.isForComparisonOnly).toBe(true);
    expect(modified.activeFrom).toBeNull();
});

it("should wrap an add failure in a view service exception", async () => {
    const providerViewService = new ProviderViewService(
        createProviderService({ addProviderAsync: rejects }));

    await expect(providerViewService.addProviderAsync(
        providerViewService.createProviderFormValues()))
        .rejects.toThrowError("We could not add this provider, please correct any errors and try again.");
});

it("should wrap an update failure in a view service exception", async () => {
    const providerViewService = new ProviderViewService(
        createProviderService({ modifyProviderAsync: rejects }));

    await expect(providerViewService.updateProviderAsync(
        "8f4b2c26-0c0d-4a0e-9f2b-2a2f4a6c1111",
        providerViewService.createProviderFormValues()))
        .rejects.toThrowError("We could not save this provider, please correct any errors and try again.");
});

it("should wrap a delete failure in a view service exception", async () => {
    const providerViewService = new ProviderViewService(
        createProviderService({ removeProviderByIdAsync: rejects }));

    await expect(providerViewService.removeProviderAsync("8f4b2c26-0c0d-4a0e-9f2b-2a2f4a6c1111"))
        .rejects.toThrowError("We could not delete this provider, please try again or contact support.");
});
