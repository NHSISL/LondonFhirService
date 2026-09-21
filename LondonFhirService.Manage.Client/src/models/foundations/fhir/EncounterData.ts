export type EncounterData = {
    id: string;
    display: string | null;
    code: string | null;
    system: string | null;
    status: string | null;
    periodStart: string | null;
    periodEnd: string | null;
    serviceProviderRef: string | null;
    subjectRef: string | null;
    participantRefs: string[];
};
