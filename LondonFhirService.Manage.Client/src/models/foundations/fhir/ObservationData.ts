export type ObservationData = {
    id: string;
    display: string | null;
    code: string | null;
    system: string | null;
    category: string | null;
    status: string | null;
    value: string | null;
    valueQuantity: number | null;
    unit: string | null;
    effectiveDateTime: string | null;
    effectivePeriodStart: string | null;
    performerRefs: string[];
};
