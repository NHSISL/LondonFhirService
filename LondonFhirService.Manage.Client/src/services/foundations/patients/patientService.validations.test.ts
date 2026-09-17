import { expect, it } from "vitest";
import { isCompleteDate, validateStructuredRecordRequest } from "./patientService.validations";
import type { StructuredRecordRequest } from "../../../models/foundations/patients/StructuredRecordRequest";

const request = (overrides: Partial<StructuredRecordRequest>): StructuredRecordRequest => ({
    clientId: "",
    clientSecret: "",
    scope: "",
    grantType: "client_credentials",
    nhsNumber: "9000000009",
    dateOfBirth: "",
    demographicsOnly: false,
    ...overrides
});

it("should accept a request carrying only an NHS number", () => {
    // The credentials are optional by design - a blank one falls back to what the host is
    // configured with - so a request with none of them filled in is valid.
    expect(() => validateStructuredRecordRequest(request({}))).not.toThrow();
});

it("should reject a blank NHS number", () => {
    expect(() => validateStructuredRecordRequest(request({ nhsNumber: "  " })))
        .toThrowError("nhsNumber: An NHS number is required.");
});

it("should accept a well formed date of birth", () => {
    expect(() => validateStructuredRecordRequest(request({ dateOfBirth: "2002-10-01" })))
        .not.toThrow();
});

it("should reject a date of birth in the wrong format", () => {
    expect(() => validateStructuredRecordRequest(request({ dateOfBirth: "01-10-2002" })))
        .toThrowError("dateOfBirth: A date of birth must be a valid date in the format YYYY-MM-DD.");
});

it("should reject a date that matches the format but does not exist", () => {
    // Date.UTC rolls 30 February forward into March rather than failing, so a format check alone
    // would let this through and the provider would answer a bare 400.
    expect(() => validateStructuredRecordRequest(request({ dateOfBirth: "2002-02-30" })))
        .toThrowError("dateOfBirth: A date of birth must be a valid date in the format YYYY-MM-DD.");
});

it("should tell a real date from a rolled one", () => {
    expect(isCompleteDate("2004-02-29")).toBe(true);
    expect(isCompleteDate("2003-02-29")).toBe(false);
    expect(isCompleteDate("2002-13-01")).toBe(false);
    expect(isCompleteDate("2002-10-1")).toBe(false);
    expect(isCompleteDate("")).toBe(false);
});
