// Foundation model - mirrors LondonFhirService.Core.Models.Foundations.Metrics.Metric as the
// /api/metrics endpoint serialises it.
//
// type and status arrive as ORDINALS, not names. The Manage host registers no
// JsonStringEnumConverter, so although EF persists both as text, on the wire they are numbers.
export type Metric = {
    id: string;
    userId: string | null;
    parentId: string | null;
    correlationId: string;
    method: string;
    type: number;
    name: string;
    target: string | null;
    started: string;
    completed: string;
    durationMs: number;
    status: number;
    errorCode: string | null;
    payloadBytes: number | null;
    consumer: string | null;
    description: string | null;
    createdDate: string;
};
