import { expect, it, vi } from "vitest";
import { PatientApiBrokerException } from "../../../models/foundations/patients/exceptions/PatientApiBrokerException";
import { PatientDependencyException } from "../../../models/foundations/patients/exceptions/PatientDependencyException";
import { PatientValidationException } from "../../../models/foundations/patients/exceptions/PatientValidationException";
import { StructuredRecordViewService } from "./structuredRecordViewService";
import {
    StructuredRecordViewServiceException
} from "../../../models/views/patients/exceptions/StructuredRecordViewServiceException";
import type { IPatientService } from "../../foundations/patients/iPatientService";
import type { StructuredRecordFormValues } from "../../../models/views/patients/StructuredRecordFormValues";

const formValues = (
    overrides: Partial<StructuredRecordFormValues> = {})
    : StructuredRecordFormValues => ({
    clientId: "",
    clientSecret: "",
    scope: "",
    grantType: "client_credentials",
    nhsNumber: "9000000009",
    dateOfBirth: "",
    demographicsOnly: false,
    ...overrides
});

const patientServiceReturning = (payload: string): IPatientService => ({
    retrieveStructuredRecordAsync: vi.fn().mockResolvedValue({
        payloadText: payload,
        correlationId: "9f2c41be7a0d4e5bb6c8d3117e42a905"
    })
});

it("should pre-fill the grant type and nothing else", () => {
    const viewService = new StructuredRecordViewService(patientServiceReturning(""));

    expect(viewService.createStructuredRecordFormValues()).toEqual({
        clientId: "",
        clientSecret: "",
        scope: "",
        grantType: "client_credentials",
        nhsNumber: "",
        dateOfBirth: "",
        demographicsOnly: false
    });
});

it("should pretty print a JSON payload", async () => {
    const viewService = new StructuredRecordViewService(
        patientServiceReturning("{\"resourceType\":\"Bundle\",\"type\":\"collection\"}"));

    const view = await viewService.retrieveStructuredRecordViewAsync(formValues());

    expect(view.isJson).toBe(true);
    expect(view.payloadText).toContain("\n  \"resourceType\": \"Bundle\"");
    expect(view.lineCount).toBe(4);
});

it("should show a payload that is not JSON as it arrived", async () => {
    // The case an operator opened this page to see. It must stay visible rather than becoming an
    // error about unparseable content.
    const viewService = new StructuredRecordViewService(
        patientServiceReturning("<html><body>502 Bad Gateway</body></html>"));

    const view = await viewService.retrieveStructuredRecordViewAsync(formValues());

    expect(view.isJson).toBe(false);
    expect(view.payloadText).toBe("<html><body>502 Bad Gateway</body></html>");
});

it("should trim the fields it sends and leave blank ones blank", async () => {
    const patientService = patientServiceReturning("{}");
    const viewService = new StructuredRecordViewService(patientService);

    await viewService.retrieveStructuredRecordViewAsync(formValues({
        clientId: "  a-client-id  ",
        nhsNumber: " 9000000009 "
    }));

    expect(patientService.retrieveStructuredRecordAsync).toHaveBeenCalledWith(
        {
            clientId: "a-client-id",
            clientSecret: "",
            scope: "",
            grantType: "client_credentials",
            nhsNumber: "9000000009",
            dateOfBirth: "",
            demographicsOnly: false
        },
        undefined);
});

it("should let a validation failure through unwrapped", async () => {
    const patientService: IPatientService = {
        retrieveStructuredRecordAsync: vi.fn().mockRejectedValue(
            new PatientValidationException("nhsNumber", "An NHS number is required."))
    };

    const viewService = new StructuredRecordViewService(patientService);

    // Wrapping it would tell someone who left a field blank to contact support.
    await expect(viewService.retrieveStructuredRecordViewAsync(formValues()))
        .rejects.toBeInstanceOf(PatientValidationException);
});

it("should wrap anything else as a view service failure", async () => {
    const patientService: IPatientService = {
        retrieveStructuredRecordAsync: vi.fn().mockRejectedValue(new Error("socket hang up"))
    };

    const viewService = new StructuredRecordViewService(patientService);

    await expect(viewService.retrieveStructuredRecordViewAsync(formValues()))
        .rejects.toBeInstanceOf(StructuredRecordViewServiceException);
});

it("should surface what the API said rather than a generic message", async () => {
    // The page exists to tell an operator what happened. A 400 naming the field they got wrong
    // must reach them, not be replaced with "contact support".
    const patientService: IPatientService = {
        retrieveStructuredRecordAsync: vi.fn().mockRejectedValue(
            new PatientDependencyException(
                "Patient dependency error occurred, please contact support.",
                new PatientApiBrokerException(
                    "The API answered 400: nhsNumber: Text is invalid",
                    new Error("Request failed with status code 400"))))
    };

    const viewService = new StructuredRecordViewService(patientService);

    await expect(viewService.retrieveStructuredRecordViewAsync(formValues()))
        .rejects.toThrowError("The API answered 400: nhsNumber: Text is invalid");
});

it("should fall back to the generic message when there is nothing from the API", async () => {
    const patientService: IPatientService = {
        retrieveStructuredRecordAsync: vi.fn().mockRejectedValue(new Error("socket hang up"))
    };

    const viewService = new StructuredRecordViewService(patientService);

    await expect(viewService.retrieveStructuredRecordViewAsync(formValues()))
        .rejects.toThrowError("please try again or contact support");
});
