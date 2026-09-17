import { expect, it, vi } from "vitest";
import ApiBroker from "../apiBroker";
import { PatientApiBroker } from "./patientApiBroker";
import type { StructuredRecordRequest } from "../../models/foundations/patients/StructuredRecordRequest";

const structuredRecordRequest: StructuredRecordRequest = {
    clientId: "",
    clientSecret: "",
    scope: "",
    grantType: "",
    nhsNumber: "9435797881",
    dateOfBirth: "1994-05-21",
    demographicsOnly: false
};

const brokerRejecting = (status: number, data: unknown): PatientApiBroker => {
    const apiBroker = {
        PostAsync: vi.fn().mockRejectedValue({ response: { status: status, data: data } })
    } as unknown as ApiBroker;

    return new PatientApiBroker(apiBroker);
};

const brokerResolving = (data: unknown, headers: unknown = {}): PatientApiBroker => {
    const apiBroker = {
        PostAsync: vi.fn().mockResolvedValue({ data: data, headers: headers })
    } as unknown as ApiBroker;

    return new PatientApiBroker(apiBroker);
};

const messageOf = async (broker: PatientApiBroker): Promise<string> => {
    try {
        await broker.postStructuredRecordAsync(structuredRecordRequest);
    } catch (exception) {
        return (exception as Error).message;
    }

    throw new Error("Expected postStructuredRecordAsync to reject.");
};

// The reason detail is read before title. On an upstream refusal PatientsController sets title to
// its own generic wrapper and detail to the token endpoint's actual words, so reading title first
// would show the operator the wrapper and discard the only part that says what went wrong.
it("should prefer the upstream detail over this service's own title", async () => {
    const message = await messageOf(brokerRejecting(400, {
        title: "Patient dependency validation error occurred, please fix the errors and try again.",
        status: 400,
        detail: "{\"error\":\"invalid_client\",\"error_description\":\"AADSTS7000215\"}"
    }));

    expect(message).toContain("invalid_client");
    expect(message).not.toContain("Patient dependency validation error occurred");
});

it("should carry the provider's body back from a 500 as well", async () => {
    const message = await messageOf(brokerRejecting(500, {
        title: "Patient dependency error occurred, contact support.",
        status: 500,
        detail: "{\"resourceType\":\"OperationOutcome\",\"diagnostics\":\"Upstream unavailable\"}"
    }));

    expect(message).toContain("The API answered 500");
    expect(message).toContain("Upstream unavailable");
});

// Field errors still win, because they name what the operator got wrong. A validation failure has
// no upstream detail to compete with - RESTFulSense builds that body from Exception.Data.
it("should prefer field errors when the API names the field that was wrong", async () => {
    const message = await messageOf(brokerRejecting(400, {
        title: "Patient validation error occurred, please fix errors and try again.",
        status: 400,
        errors: { nhsNumber: ["Text is required"] }
    }));

    expect(message).toContain("nhsNumber: Text is required");
});

// A 500 from a transport failure never had a response body to keep, so detail is null and the
// title is all there is.
it("should fall back to the title when there is no detail", async () => {
    const message = await messageOf(brokerRejecting(500, {
        title: "Patient dependency error occurred, contact support.",
        status: 500,
        detail: null
    }));

    expect(message).toBe("The API answered 500: Patient dependency error occurred, contact support.");
});

const brokerThrowing = (failure: unknown): PatientApiBroker => {
    const apiBroker = {
        PostAsync: vi.fn().mockRejectedValue(failure)
    } as unknown as ApiBroker;

    return new PatientApiBroker(apiBroker);
};

// These three used to share one sentence - "did not reach the API" - which asserted a network
// fact none of them established. A token the portal could not acquire throws before axios sends
// anything, and reporting that as a network failure sends the reader to the wrong place.
it("should say the request was never sent when nothing went out", async () => {
    const message = await messageOf(brokerThrowing(new Error("no account available")));

    expect(message).toContain("never sent");
    expect(message).toContain("no account available");
});

it("should say the API did not answer when the request went out and nothing came back", async () => {
    const message = await messageOf(brokerThrowing(
        { code: "ERR_NETWORK", message: "Network Error", request: {} }));

    expect(message).toContain("did not answer");
    expect(message).not.toContain("never sent");
});

it("should name a cancellation as a cancellation", async () => {
    const message = await messageOf(brokerThrowing(
        { code: "ERR_CANCELED", message: "canceled", request: {} }));

    expect(message).toContain("cancelled");
});

// Both shapes the success path can arrive in. Which one it is depends on the content type MVC
// picked for the response, so the page must not care.
it("should return a string payload unchanged", async () => {
    const payload = "not json, just text";

    await expect(brokerResolving(payload).postStructuredRecordAsync(structuredRecordRequest))
        .resolves.toEqual({ payloadText: payload, correlationId: "" });
});

// The id the page links its comparisons from. axios lowercases header names on a real call and
// hands back a plain object in tests, so both shapes have to read.
it("should read the correlation id from a plain headers object", async () => {
    const response = await brokerResolving(
        "{}", { "x-correlation-id": "9f2c41be7a0d4e5bb6c8d3117e42a905" })
        .postStructuredRecordAsync(structuredRecordRequest);

    expect(response.correlationId).toBe("9f2c41be7a0d4e5bb6c8d3117e42a905");
});

it("should read the correlation id from an AxiosHeaders-like object", async () => {
    const headers = {
        get: (name: string) => name === "x-correlation-id" ? " abc123 " : undefined
    };

    const response = await brokerResolving("{}", headers)
        .postStructuredRecordAsync(structuredRecordRequest);

    expect(response.correlationId).toBe("abc123");
});

// An older build of the host sends no such header, and the page must not offer a link to nowhere.
it("should report an empty correlation id when the host sent no header", async () => {
    const response = await brokerResolving("{}")
        .postStructuredRecordAsync(structuredRecordRequest);

    expect(response.correlationId).toBe("");
});

it("should re-render a parsed payload as formatted json", async () => {
    const response = await brokerResolving({ resourceType: "Bundle", total: 1 })
        .postStructuredRecordAsync(structuredRecordRequest);

    expect(response.payloadText).toBe("{\n  \"resourceType\": \"Bundle\",\n  \"total\": 1\n}");
});

// The field keyed bag survives the broker boundary now. It used to be flattened into the message
// and the structure lost, which is why a rejected credential could only ever be shown as prose.
it("should keep the API's field errors as structured data", async () => {
    const apiBroker = {
        PostAsync: () => Promise.reject({
            request: {},
            response: {
                status: 400,
                data: {
                    errors: {
                        // PascalCase as the host's serialiser may send it - normalised on the way
                        // in so the page can key by its own field names either way.
                        ClientId: ["Text is invalid"],
                        clientSecret: ["Text is invalid"]
                    }
                }
            }
        })
    } as unknown as ApiBroker;

    const broker = new PatientApiBroker(apiBroker);

    await expect(broker.postStructuredRecordAsync(structuredRecordRequest))
        .rejects.toMatchObject({
            fieldErrors: {
                clientId: ["Text is invalid"],
                clientSecret: ["Text is invalid"]
            }
        });
});

// Nothing to key by is an empty bag rather than a missing property, so callers can read it
// without guarding first.
it("should give an empty field error bag when the failure is not a validation one", async () => {
    const apiBroker = {
        PostAsync: () => Promise.reject({
            request: {},
            response: { status: 500, data: { title: "Something failed" } }
        })
    } as unknown as ApiBroker;

    const broker = new PatientApiBroker(apiBroker);

    await expect(broker.postStructuredRecordAsync(structuredRecordRequest))
        .rejects.toMatchObject({ fieldErrors: {} });
});
