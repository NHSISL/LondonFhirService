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

const brokerResolving = (data: unknown): PatientApiBroker => {
    const apiBroker = {
        PostAsync: vi.fn().mockResolvedValue({ data: data })
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

it("should say so plainly when the request never reached the API", async () => {
    const apiBroker = {
        PostAsync: vi.fn().mockRejectedValue(new Error("Network Error"))
    } as unknown as ApiBroker;

    const message = await messageOf(new PatientApiBroker(apiBroker));

    expect(message).toBe("The structured record request did not reach the API.");
});

// Both shapes the success path can arrive in. Which one it is depends on the content type MVC
// picked for the response, so the page must not care.
it("should return a string payload unchanged", async () => {
    const payload = "not json, just text";

    await expect(brokerResolving(payload).postStructuredRecordAsync(structuredRecordRequest))
        .resolves.toBe(payload);
});

it("should re-render a parsed payload as formatted json", async () => {
    const payload = await brokerResolving({ resourceType: "Bundle", total: 1 })
        .postStructuredRecordAsync(structuredRecordRequest);

    expect(payload).toBe("{\n  \"resourceType\": \"Bundle\",\n  \"total\": 1\n}");
});
