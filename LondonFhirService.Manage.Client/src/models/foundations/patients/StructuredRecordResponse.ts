// The payload and the id the call was filed under. They arrive together - the payload in the
// body, the correlation id in the X-Correlation-Id response header - so they travel together
// rather than the header being read again further up.
export type StructuredRecordResponse = {
    payloadText: string;

    // 32 hex characters, no dashes: the W3C trace id. Empty when the API did not send the header,
    // which is what an older build of the host does, so the page must cope with not having it.
    correlationId: string;
};
