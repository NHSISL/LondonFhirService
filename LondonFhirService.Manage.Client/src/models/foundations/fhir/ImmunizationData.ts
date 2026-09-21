export type ImmunizationData = {
    id: string;
    display: string | null;
    code: string | null;
    system: string | null;
    status: string | null;
    occurrenceDateTime: string | null;
    encounterRef: string | null;
    patientRef: string | null;
    practitionerRefs: string[];
};
