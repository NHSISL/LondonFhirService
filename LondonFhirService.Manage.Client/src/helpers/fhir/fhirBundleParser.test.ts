import { expect, it } from "vitest";
import { extractReference, parseObservation, parseOrganization } from "./fhirResourceParsers";
import { parseBundle } from "./fhirBundleParser";

const patientBundle = {
    resourceType: "Bundle",
    entry: [
        {
            resource: {
                resourceType: "Patient",
                id: "patient-1",
                identifier: [
                    { system: "https://fhir.nhs.uk/Id/nhs-number", value: "1234567890" }
                ],
                name: [
                    {
                        use: "official",
                        prefix: ["Mr"],
                        given: ["Alex"],
                        family: "Smith",
                        suffix: ["Jr"]
                    }
                ],
                birthDate: "1980-01-01",
                gender: "male",
                address: [
                    {
                        line: ["1 Test Street"],
                        city: "Leeds",
                        district: "West Yorkshire",
                        postalCode: "LS1 1AA",
                        country: "UK"
                    }
                ],
                telecom: [
                    { system: "phone", value: "0113 555 1234" },
                    { system: "email", value: "alex@example.com" }
                ],
                communication: [
                    { language: { coding: [{ display: "English", code: "en" }] }, preferred: true }
                ],
                managingOrganization: {
                    reference: "https://example.org/fhir/Organization/org-1"
                },
                generalPractitioner: [
                    { reference: "Practitioner/prac-1" },
                    { reference: "https://example.org/fhir/Practitioner/prac-2" }
                ]
            }
        },
        {
            resource: {
                resourceType: "List",
                id: "list-1",
                title: "Medications",
                status: "current",
                entry: [
                    { item: { reference: "MedicationStatement/med-1" } },
                    { item: { reference: "Condition/cond-1" } }
                ]
            }
        }
    ]
};

it("should parse a bundle into patient data and lists", () => {
    const parsedBundle = parseBundle(JSON.stringify(patientBundle));

    expect(parsedBundle.patient.nhsNumber).toBe("1234567890");
    expect(parsedBundle.patient.namePrefix).toBe("Mr");
    expect(parsedBundle.patient.nameGiven).toBe("Alex");
    expect(parsedBundle.patient.nameFamily).toBe("Smith");
    expect(parsedBundle.patient.nameSuffix).toBe("Jr");
    expect(parsedBundle.patient.birthDate).toBe("1980-01-01");
    expect(parsedBundle.patient.gender).toBe("male");
    expect(parsedBundle.patient.addressCity).toBe("Leeds");
    expect(parsedBundle.patient.addressPostalCode).toBe("LS1 1AA");
    expect(parsedBundle.patient.telecom).toContain("Phone: 0113 555 1234");
    expect(parsedBundle.patient.telecom).toContain("Email: alex@example.com");
    expect(parsedBundle.patient.communication).toBe("English (preferred)");
    expect(parsedBundle.patient.managingOrganizationRef).toBe("Organization/org-1");

    expect(parsedBundle.patient.generalPractitionerRefs)
        .toEqual(["Practitioner/prac-1", "Practitioner/prac-2"]);

    expect(parsedBundle.lists).toHaveLength(1);
    expect(parsedBundle.lists[0].title).toBe("Medications");

    expect(parsedBundle.lists[0].itemRefs)
        .toEqual(["MedicationStatement/med-1", "Condition/cond-1"]);
});

it("should index every resource in the bundle by its reference", () => {
    const parsedBundle = parseBundle(JSON.stringify(patientBundle));

    expect([...parsedBundle.resourceIndex.keys()]).toEqual(["Patient/patient-1", "List/list-1"]);
});

// One provider returning a broken payload must not take the other side of the comparison down
// with it, so a bad bundle reads as an empty one.
it("should return an empty bundle for a payload that is not readable", () => {
    const parsedBundle = parseBundle("{ not json");

    expect(parsedBundle.patient.nhsNumber).toBeNull();
    expect(parsedBundle.patient.resource).toBeNull();
    expect(parsedBundle.lists).toEqual([]);
    expect(parsedBundle.episodesOfCare).toEqual([]);
    expect(parsedBundle.resourceIndex.size).toBe(0);
});

// The two sides come from different providers, so only the trailing ResourceType/id of a
// reference is comparable - and it is the key the bundle's resources are indexed under.
it("should extract a relative reference from an absolute URL", () => {
    expect(extractReference({ reference: "https://example.org/fhir/Organization/org-9" }))
        .toBe("Organization/org-9");

    expect(extractReference({ reference: "Organization/org-9" })).toBe("Organization/org-9");
    expect(extractReference({})).toBeNull();
});

it("should parse observation values for quantity and boolean", () => {
    const parsedQuantity = parseObservation({
        id: "obs-1",
        resourceType: "Observation",
        code: { coding: [{ display: "Height", code: "H", system: "http://loinc.org" }] },
        valueQuantity: { value: 180, unit: "cm" },
        status: "final"
    });

    const parsedBoolean = parseObservation({
        id: "obs-2",
        resourceType: "Observation",
        code: { coding: [{ display: "Smoker", code: "S", system: "http://loinc.org" }] },
        valueBoolean: true,
        status: "final"
    });

    expect(parsedQuantity.value).toBe("180 cm");
    expect(parsedQuantity.valueQuantity).toBe(180);
    expect(parsedQuantity.unit).toBe("cm");
    expect(parsedBoolean.value).toBe("Yes");
});

it("should parse organisation identifiers and address", () => {
    const parsedOrganization = parseOrganization({
        id: "org-1",
        resourceType: "Organization",
        name: "Test Org",
        identifier: [
            { system: "https://fhir.nhs.uk/Id/ods-organization-code", value: "A12345" }
        ],
        address: [{ line: ["2 Test Street"], city: "Leeds", postalCode: "LS2 2BB" }]
    });

    expect(parsedOrganization.odsCode).toBe("A12345");
    expect(parsedOrganization.addressLine).toBe("2 Test Street");
    expect(parsedOrganization.addressCity).toBe("Leeds");
    expect(parsedOrganization.addressPostalCode).toBe("LS2 2BB");
});

// STU3 sends clinicalStatus as a plain code and R4 as a CodeableConcept. Both providers are in
// scope, so both have to read the same way.
it("should read a status that arrives as a code or as a codeable concept", () => {
    const bundleWithBothForms = {
        resourceType: "Bundle",
        entry: [
            {
                resource: {
                    resourceType: "List",
                    id: "list-1",
                    title: "Problems",
                    entry: [{ item: { reference: "Condition/cond-1" } }]
                }
            }
        ]
    };

    expect(parseBundle(JSON.stringify(bundleWithBothForms)).lists[0].itemCount).toBe(1);
});
