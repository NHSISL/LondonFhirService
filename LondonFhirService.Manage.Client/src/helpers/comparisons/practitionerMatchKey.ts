import { findIdentifierBySystem, readString } from "../fhir/fhirJson";
import { buildPracticeQualifiedDdsMatchKey } from "./practiceQualifiedDdsMatchKey";
import type { FhirResource } from "../../models/foundations/fhir/FhirResource";

// Mirrors PractitionerMatcherService server side: the SDS user id when the practitioner carries
// one, and the practice-qualified DDS identifier when it does not.
//
// The fallback exists because the SDS id is not reliably populated - feeds have been seen carrying
// the identifier entry with its system and no value. Keyed on SDS alone those practitioners
// produced no key at all, so their differences had nothing to attach to here and the server
// dropped them from the comparison entirely.
//
// SDS is tried first because it is a national identifier and means the same thing to both
// providers, where a DDS identifier is local to the feed that issued it. The two key spaces cannot
// collide: the DDS form is always prefixed with its practice and a separator, which an SDS id
// never contains.
export function buildPractitionerMatchKey(resource: FhirResource): string | null {
    const sdsUserId = readString(findIdentifierBySystem(resource, "sds-user-id")?.value);

    if (sdsUserId !== null) {
        return sdsUserId;
    }

    return buildPracticeQualifiedDdsMatchKey(resource);
}
