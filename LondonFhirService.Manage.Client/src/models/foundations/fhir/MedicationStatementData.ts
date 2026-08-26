export type MedicationStatementData = {
    id: string;
    medicationName: string | null;
    medicationCode: string | null;
    medicationSystem: string | null;
    dosage: string | null;
    status: string | null;
    dateAsserted: string | null;
    informationSourceRef: string | null;
    medicationRef: string | null;
};
