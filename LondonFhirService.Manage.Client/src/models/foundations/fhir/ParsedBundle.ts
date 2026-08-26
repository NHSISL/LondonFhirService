import type { EpisodeOfCareData } from "./EpisodeOfCareData";
import type { FhirResourceIndex } from "./FhirResource";
import type { ListData } from "./ListData";
import type { PatientData } from "./PatientData";

// What one side of a comparison is reduced to before it is rendered. The index keeps every
// resource in the bundle reachable by reference, so a List entry or a careManager can be resolved
// on demand rather than eagerly parsed.
export type ParsedBundle = {
    patient: PatientData;
    resourceIndex: FhirResourceIndex;
    lists: ListData[];
    episodesOfCare: EpisodeOfCareData[];
};
