import { findIdentifierBySystem, readObjectArray, readPath, readString } from "../fhir/fhirJson";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const ddsIdentifierSystem = "https://fhir.hl7.org.uk/Id/dds";
const odsCodeSystem = "https://fhir.nhs.uk/Id/ODS-Code";

// Mirrors PracticeQualifiedDdsMatchKey server side, which every matcher that pairs on a DDS
// identifier uses - Observation, FamilyMemberHistory, Immunization, Encounter, MedicationRequest,
// DiagnosticReport, Procedure, ProcedureRequest, ReferralRequest and Appointment - so a difference
// the engine recorded against one of these resources can be shown on that resource's own row.
//
// A DDS identifier is unique within one practice's feed, not within a bundle: a patient
// registered at two practices carries two unrelated resources both claiming DDS id '54'. Keying
// on the bare identifier collided, and the engine reported those collisions as
// manual-review-required rather than pairing them. The ODS code off meta.tag separates them.
//
// A resource with no ODS tag keys on an empty prefix rather than returning null - the same as the
// server - so untagged resources group exactly as they did before this existed.
//
// This has to stay in step with the server: the key is how a difference finds the row it belongs
// to, so a change to one side without the other leaves differences unattached and they fall into
// the Other differences list instead.
export function buildPracticeQualifiedDdsMatchKey(resource: FhirResource): string | null {
    const ddsIdentifier = readString(findIdentifierBySystem(resource, ddsIdentifierSystem)?.value);

    if (ddsIdentifier === null) {
        return null;
    }

    return `${readOdsCode(resource) ?? ""}|${ddsIdentifier}`;
}

function readOdsCode(resource: FhirResource): string | null {
    const tags = readObjectArray(readPath(resource, "meta", "tag"));

    for (const tag of tags) {
        if (readString(tag.system) === odsCodeSystem) {
            return readString(tag.code);
        }
    }

    return null;
}
