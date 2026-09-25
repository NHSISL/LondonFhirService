import { act, renderHook } from "@testing-library/react";
import { expect, it } from "vitest";
import { useValidation } from "./useValidation";
import type { Validation } from "../models/validations/validation";

type NameErrors = { hasErrors: boolean; name: string };
type NameApiErrors = { name: string[] };

const noErrors: NameErrors = { hasErrors: false, name: "" };
const nameIsRequired: Validation[] = [{ friendlyName: "Name", property: "name", isRequired: true }];

const renderValidation = (initialValues: { name: string }) =>
    renderHook(
        ({ values }) => useValidation<NameErrors, NameApiErrors>(noErrors, nameIsRequired, values),
        { initialProps: { values: initialValues } });

it("should show no errors until validation is turned on", () => {
    const rendered = renderValidation({ name: "" });

    expect(rendered.result.current.errors).toEqual(noErrors);

    act(() => rendered.result.current.enableValidationMessages());

    expect(rendered.result.current.errors.name.length).toBeGreaterThan(0);
    expect(rendered.result.current.errors.hasErrors).toBe(true);
});

it("should re-validate as the values change while validation is on", () => {
    const rendered = renderValidation({ name: "" });
    act(() => rendered.result.current.enableValidationMessages());

    rendered.rerender({ values: { name: "Ada" } });

    expect(rendered.result.current.errors.name).toBe("");
    expect(rendered.result.current.errors.hasErrors).toBe(false);
});

it("should answer a submit straight away, before the next render", () => {
    const rendered = renderValidation({ name: "" });

    let hasErrors = false;
    act(() => { hasErrors = rendered.result.current.validate({ name: "" }); });

    expect(hasErrors).toBe(true);
});

it("should show API errors until the values move on, then fall back to validation", () => {
    const values = { name: "Ada" };
    const rendered = renderValidation(values);
    act(() => rendered.result.current.enableValidationMessages());

    act(() => rendered.result.current.processApiErrors({ name: ["Already taken"] }));

    expect(rendered.result.current.errors.name).toBe("Already taken");

    rendered.rerender({ values: { name: "Grace" } });

    expect(rendered.result.current.errors.name).toBe("");
});

it("should clear everything when validation is turned off", () => {
    const rendered = renderValidation({ name: "" });
    act(() => rendered.result.current.enableValidationMessages());
    act(() => rendered.result.current.processApiErrors({ name: ["Already taken"] }));

    act(() => rendered.result.current.disableValidationMessages());

    expect(rendered.result.current.errors).toEqual(noErrors);
});
