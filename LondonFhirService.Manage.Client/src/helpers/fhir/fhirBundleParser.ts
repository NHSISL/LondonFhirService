import { extractReference, joinOrNull, parseEpisodeOfCare, parseList } from "./fhirResourceParsers";
import {
    findIdentifierBySystem,
    readFirstCoding,
    readObject,
    readObjectArray,
    readString,
    readStringArray
} from "./fhirJson";
import type { EpisodeOfCareData } from "../../models/foundations/fhir/EpisodeOfCareData";
import type { FhirResource, FhirResourceIndex } from "../../models/foundations/fhir/FhirResource";
import type { ListData } from "../../models/foundations/fhir/ListData";
import type { ParsedBundle } from "../../models/foundations/fhir/ParsedBundle";
import type { PatientData } from "../../models/foundations/fhir/PatientData";

const emptyPatientData: PatientData = {
    nhsNumber: null,
    namePrefix: null,
    nameGiven: null,
    nameFamily: null,
    nameSuffix: null,
    birthDate: null,
    gender: null,
    addressLine: null,
    addressCity: null,
    addressDistrict: null,
    addressPostalCode: null,
    addressCountry: null,
    telecom: null,
    communication: null,
    managingOrganizationRef: null,
    generalPractitionerRefs: [],
    resource: null
};

// A payload that will not parse is a real outcome here rather than a bug to throw on: one side of
// a comparison can be malformed while the other is fine, and the page still has to render the
// good side and show that the other is empty.
export function parseBundle(jsonPayload: string): ParsedBundle {
    const emptyBundle: ParsedBundle = {
        patient: emptyPatientData,
        resourceIndex: new Map(),
        lists: [],
        episodesOfCare: []
    };

    const bundle = tryParseJson(jsonPayload);

    if (bundle === null) {
        return emptyBundle;
    }

    const resources = readObjectArray(bundle.entry)
        .map(entry => readObject(entry.resource))
        .filter((resource): resource is FhirResource => resource !== null);

    const resourceIndex: FhirResourceIndex = new Map();
    const lists: ListData[] = [];
    const episodesOfCare: EpisodeOfCareData[] = [];
    let patientResource: FhirResource | null = null;

    for (const resource of resources) {
        const resourceType = readString(resource.resourceType);
        const id = readString(resource.id);

        if (resourceType !== null && id !== null) {
            resourceIndex.set(`${resourceType}/${id}`, resource);
        }

        if (resourceType === "List") {
            lists.push(parseList(resource));
        }

        if (resourceType === "EpisodeOfCare") {
            episodesOfCare.push(parseEpisodeOfCare(resource));
        }

        if (resourceType === "Patient" && patientResource === null) {
            patientResource = resource;
        }
    }

    return {
        patient: patientResource === null
            ? emptyPatientData
            : parsePatient(patientResource),

        resourceIndex: resourceIndex,
        lists: lists,
        episodesOfCare: episodesOfCare
    };
}

function parsePatient(resource: FhirResource): PatientData {
    const names = readObjectArray(resource.name);
    const nameToUse = names.find(name => readString(name.use) === "official") ?? names[0] ?? null;
    const address = readObjectArray(resource.address)[0] ?? null;
    const nhsNumberIdentifier = findIdentifierBySystem(resource, "nhs-number");

    const generalPractitionerRefs = readObjectArray(resource.generalPractitioner)
        .map(generalPractitioner => extractReference(generalPractitioner))
        .filter((reference): reference is string => reference !== null);

    return {
        nhsNumber: readString(nhsNumberIdentifier?.value),
        namePrefix: joinOrNull(readStringArray(nameToUse?.prefix), " "),
        nameGiven: joinOrNull(readStringArray(nameToUse?.given), " "),
        nameFamily: readString(nameToUse?.family),
        nameSuffix: joinOrNull(readStringArray(nameToUse?.suffix), " "),
        birthDate: readString(resource.birthDate),
        gender: readString(resource.gender),
        addressLine: joinOrNull(readStringArray(address?.line), ", "),
        addressCity: readString(address?.city),
        addressDistrict: readString(address?.district),
        addressPostalCode: readString(address?.postalCode),
        addressCountry: readString(address?.country),
        telecom: formatTelecom(resource),
        communication: formatCommunication(resource),
        managingOrganizationRef: extractReference(resource.managingOrganization),
        generalPractitionerRefs: generalPractitionerRefs,
        resource: resource
    };
}

function formatTelecom(resource: FhirResource): string | null {
    const formattedTelecoms = readObjectArray(resource.telecom)
        .map(telecom => {
            const value = readString(telecom.value);

            if (value === null) {
                return null;
            }

            const system = readString(telecom.system);

            if (system === "phone") {
                return `Phone: ${value}`;
            }

            if (system === "email") {
                return `Email: ${value}`;
            }

            return value;
        })
        .filter((telecom): telecom is string => telecom !== null);

    return joinOrNull(formattedTelecoms, "; ");
}

function formatCommunication(resource: FhirResource): string | null {
    const formattedCommunications = readObjectArray(resource.communication)
        .map(communication => {
            const languageCoding = readFirstCoding(communication.language);

            if (languageCoding === null) {
                return null;
            }

            const language =
                readString(languageCoding.display) ?? readString(languageCoding.code) ?? "";

            return `${language} ${communication.preferred === true ? "(preferred)" : ""}`.trim()
                || null;
        })
        .filter((communication): communication is string => communication !== null);

    return joinOrNull(formattedCommunications, "; ");
}

function tryParseJson(jsonPayload: string): FhirResource | null {
    if (jsonPayload.trim().length === 0) {
        return null;
    }

    try {
        return readObject(JSON.parse(jsonPayload));
    } catch {
        return null;
    }
}
