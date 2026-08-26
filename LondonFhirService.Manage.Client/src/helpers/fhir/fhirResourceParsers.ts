import {
    findIdentifierBySystem,
    readCode,
    readFirstCoding,
    readNumber,
    readObject,
    readObjectArray,
    readPath,
    readString,
    readStringArray
} from "./fhirJson";
import type { AllergyIntoleranceData } from "../../models/foundations/fhir/AllergyIntoleranceData";
import type { ConditionData } from "../../models/foundations/fhir/ConditionData";
import type { EpisodeOfCareData } from "../../models/foundations/fhir/EpisodeOfCareData";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";
import type { ListData } from "../../models/foundations/fhir/ListData";
import type { MedicationData } from "../../models/foundations/fhir/MedicationData";
import type { MedicationStatementData } from "../../models/foundations/fhir/MedicationStatementData";
import type { ObservationData } from "../../models/foundations/fhir/ObservationData";
import type { OrganizationData } from "../../models/foundations/fhir/OrganizationData";
import type { PractitionerData } from "../../models/foundations/fhir/PractitionerData";
import type { PractitionerRoleData } from "../../models/foundations/fhir/PractitionerRoleData";

// A Reference can be relative ("Organization/1") or absolute
// ("https://provider.example/fhir/Organization/1"). The two sides of a comparison come from
// different providers and so use different base URLs, which means only the trailing
// "ResourceType/id" is comparable - and it is also the key the bundle's own resources are indexed
// under.
export function extractReference(reference: unknown): string | null {
    const rawReference = readString(readObject(reference)?.reference);

    if (rawReference === null) {
        return null;
    }

    if (rawReference.includes("://") === false) {
        return rawReference;
    }

    const parts = rawReference.split("/");

    return parts.length >= 2
        ? `${parts[parts.length - 2]}/${parts[parts.length - 1]}`
        : rawReference;
}

export function parseList(resource: FhirResource): ListData {
    const itemRefs = readObjectArray(resource.entry)
        .map(entry => extractReference(entry.item))
        .filter((itemRef): itemRef is string => itemRef !== null);

    return {
        id: readString(resource.id) ?? "",
        title: readString(resource.title) ?? "Untitled List",
        status: readString(resource.status),
        itemCount: itemRefs.length,
        itemRefs: itemRefs
    };
}

export function parseEpisodeOfCare(resource: FhirResource): EpisodeOfCareData {
    const typeCoding = readFirstCoding(readObjectArray(resource.type)[0]);

    return {
        id: readString(resource.id) ?? "",
        status: readString(resource.status),
        typeDisplay: readString(typeCoding?.display),
        typeCode: readString(typeCoding?.code),
        careManagerRef: extractReference(resource.careManager),
        organizationRef: extractReference(resource.managingOrganization),
        periodStart: readString(readPath(resource, "period", "start"))
    };
}

export function parseOrganization(resource: FhirResource): OrganizationData {
    const odsIdentifier = findIdentifierBySystem(resource, "ods-organization-code");
    const address = readObjectArray(resource.address)[0] ?? null;

    return {
        id: readString(resource.id) ?? "",
        name: readString(resource.name),
        odsCode: readString(odsIdentifier?.value),
        odsSystem: readString(odsIdentifier?.system),
        addressLine: joinOrNull(readStringArray(address?.line), ", "),
        addressCity: readString(address?.city),
        addressPostalCode: readString(address?.postalCode)
    };
}

export function parsePractitioner(resource: FhirResource): PractitionerData {
    const sdsIdentifier = findIdentifierBySystem(resource, "sds-user-id");
    const ddsIdentifier = findIdentifierBySystem(resource, "/dds");
    const names = readObjectArray(resource.name);
    const nameToUse = names.find(name => readString(name.use) === "official") ?? names[0] ?? null;

    return {
        id: readString(resource.id) ?? "",
        displayName: nameToUse === null ? null : formatHumanName(nameToUse),
        sdsUserId: readString(sdsIdentifier?.value),
        sdsSystem: readString(sdsIdentifier?.system),
        ddsId: readString(ddsIdentifier?.value),
        ddsSystem: readString(ddsIdentifier?.system)
    };
}

export function parsePractitionerRole(resource: FhirResource): PractitionerRoleData {
    const roleCoding = readObjectArray(resource.code)
        .map(code => readFirstCoding(code))
        .find((coding): coding is FhirResource => coding !== null)
        ?? null;

    return {
        id: readString(resource.id) ?? "",
        roleDisplay: readString(roleCoding?.display),
        roleCode: readString(roleCoding?.code),
        roleSystem: readString(roleCoding?.system),
        practitionerRef: extractReference(resource.practitioner),
        organizationRef: extractReference(resource.organization)
    };
}

export function parseMedication(resource: FhirResource): MedicationData {
    const coding = readFirstCoding(resource.code);

    return {
        id: readString(resource.id) ?? "",
        display: readString(coding?.display) ?? readString(readPath(resource, "code", "text")),
        code: readString(coding?.code),
        system: readString(coding?.system)
    };
}

export function parseCondition(resource: FhirResource): ConditionData {
    const coding = readFirstCoding(resource.code);

    return {
        id: readString(resource.id) ?? "",
        display: readString(coding?.display) ?? readString(readPath(resource, "code", "text")),
        code: readString(coding?.code),
        system: readString(coding?.system),
        clinicalStatus: readCode(resource.clinicalStatus),
        onsetDateTime: readString(resource.onsetDateTime),
        significance: readProblemSignificance(resource)
    };
}

export function parseAllergyIntolerance(resource: FhirResource): AllergyIntoleranceData {
    const coding = readFirstCoding(resource.code);

    return {
        id: readString(resource.id) ?? "",
        display: readString(coding?.display) ?? readString(readPath(resource, "code", "text")),
        code: readString(coding?.code),
        system: readString(coding?.system),
        clinicalStatus: readCode(resource.clinicalStatus),
        type: readCode(resource.type),
        verificationStatus: readCode(resource.verificationStatus),
        onsetDateTime: readString(resource.onsetDateTime),
        asserterRef: extractReference(resource.asserter)
    };
}

export function parseMedicationStatement(resource: FhirResource): MedicationStatementData {
    const medicationCoding = readFirstCoding(resource.medicationCodeableConcept);

    return {
        id: readString(resource.id) ?? "",

        medicationName: readString(medicationCoding?.display)
            ?? readString(readPath(resource, "medicationCodeableConcept", "text")),

        medicationCode: readString(medicationCoding?.code),
        medicationSystem: readString(medicationCoding?.system),
        dosage: readString(readObjectArray(resource.dosage)[0]?.text),
        status: readString(resource.status),
        dateAsserted: readString(resource.dateAsserted),
        informationSourceRef: extractReference(resource.informationSource),
        medicationRef: extractReference(resource.medicationReference)
    };
}

export function parseObservation(resource: FhirResource): ObservationData {
    const coding = readFirstCoding(resource.code);
    const category = readObjectArray(resource.category)[0] ?? null;
    const valueQuantity = readObject(resource.valueQuantity);

    const performerRefs = readObjectArray(resource.performer)
        .map(performer => extractReference(performer))
        .filter((performerRef): performerRef is string => performerRef !== null);

    return {
        id: readString(resource.id) ?? "",
        display: readString(coding?.display) ?? readString(readPath(resource, "code", "text")),
        code: readString(coding?.code),
        system: readString(coding?.system),

        category: readString(category?.text)
            ?? readString(readFirstCoding(category)?.display),

        status: readString(resource.status),
        value: readObservationValue(resource),
        valueQuantity: readNumber(valueQuantity?.value),
        unit: readString(valueQuantity?.unit),
        effectiveDateTime: readString(resource.effectiveDateTime),
        effectivePeriodStart: readString(readPath(resource, "effectivePeriod", "start")),
        performerRefs: performerRefs
    };
}

export function formatHumanName(humanName: FhirResource): string | null {
    const parts = [
        joinOrNull(readStringArray(humanName.prefix), " "),
        joinOrNull(readStringArray(humanName.given), " "),
        readString(humanName.family),
        joinOrNull(readStringArray(humanName.suffix), " ")
    ].filter((part): part is string => part !== null);

    return parts.length > 0 ? parts.join(" ") : null;
}

export function joinOrNull(values: string[], separator: string): string | null {
    return values.length > 0 ? values.join(separator) : null;
}

// An Observation carries its value in one of a family of value[x] elements, only one of which is
// present. They are read in the order the POC established, so the display text does not change.
function readObservationValue(resource: FhirResource): string | null {
    const valueQuantity = readObject(resource.valueQuantity);

    if (valueQuantity !== null) {
        const quantity = readNumber(valueQuantity.value);
        const unit = readString(valueQuantity.unit);

        return `${quantity ?? ""} ${unit ?? ""}`.trim() || null;
    }

    const valueString = readString(resource.valueString);

    if (valueString !== null) {
        return valueString;
    }

    if (typeof resource.valueBoolean === "boolean") {
        return resource.valueBoolean ? "Yes" : "No";
    }

    const valueInteger = readNumber(resource.valueInteger);

    if (valueInteger !== null) {
        return String(valueInteger);
    }

    const valueDateTime = readString(resource.valueDateTime);

    if (valueDateTime !== null) {
        const parsedDate = new Date(valueDateTime);

        return Number.isNaN(parsedDate.getTime())
            ? valueDateTime
            : parsedDate.toLocaleString("en-GB");
    }

    return readCode(resource.valueCodeableConcept);
}

// Problem significance is an NHS extension rather than a core element, and the two providers spell
// its url differently, so both spellings are looked for.
function readProblemSignificance(resource: FhirResource): string | null {
    const significanceExtension = readObjectArray(resource.extension)
        .find(extension => {
            const url = readString(extension.url);

            return url !== null
                && (url.includes("primarycare-problem-significance")
                    || url.includes("problem-significance-extension"));
        })
        ?? null;

    return significanceExtension === null
        ? null
        : readCode(significanceExtension.valueCodeableConcept);
}
