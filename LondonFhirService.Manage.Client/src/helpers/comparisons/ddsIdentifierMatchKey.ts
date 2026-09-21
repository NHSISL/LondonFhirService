import { findIdentifierBySystem, readString } from "../fhir/fhirJson";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

const ddsIdentifierSystem = "https://fhir.hl7.org.uk/Id/dds";

// Mirrors the match key ObservationMatcherService, ImmunizationMatcherService,
// EncounterMatcherService and FamilyMemberHistoryMatcherService compute server side, so a
// difference the engine recorded against one of these resources can be shown on that resource's
// own row.
//
// These resources have no stable id across providers - the two sides mint their own - so the
// server matches on the identifier the DDS feed itself assigns, and writes that value as the
// difference's identifier. Returns null when the resource carries no such identifier, which is
// also when the engine recorded no identifier for it.
export function buildDdsIdentifierMatchKey(resource: FhirResource): string | null {
    return readString(findIdentifierBySystem(resource, ddsIdentifierSystem)?.value);
}
