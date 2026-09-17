import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, expect, it, vi } from "vitest";
import { useStructuredRecordPage } from "./useStructuredRecordPage";
import type { StructuredRecordView } from "../../models/views/patients/StructuredRecordView";

const retrieveStructuredRecordViewAsync = vi.fn();

vi.mock("../../services/views/patients/structuredRecordViewService", () => ({
    StructuredRecordViewService: class {
        createStructuredRecordFormValues() {
            return {
                clientId: "",
                clientSecret: "",
                scope: "",
                grantType: "client_credentials",
                nhsNumber: "",
                dateOfBirth: "",
                demographicsOnly: false
            };
        }

        retrieveStructuredRecordViewAsync(values: unknown, abortSignal?: AbortSignal) {
            return retrieveStructuredRecordViewAsync(values, abortSignal);
        }
    }
}));

// The real shape, not a cast over a stand-in. The previous fixture asserted { payload } into
// this type, and StructuredRecordView has no payload field at all - so the cast was hiding an
// object that matched the contract in no respect, and would have gone on hiding it through any
// rename or addition.
const aStructuredRecord: StructuredRecordView = {
    payloadText: "{\n  \"resourceType\": \"Bundle\"\n}",
    isJson: true,
    lineCount: 3,
    characterCountText: "30 characters",
    correlationId: "9f2c41be7a0d4e5bb6c8d3117e42a905",
    metricsUrl: "/admin/metrics/9f2c41be-7a0d-4e5b-b6c8-d3117e42a905",
    comparisonsUrl: "/admin/comparisons?correlationId=9f2c41be7a0d4e5bb6c8d3117e42a905"
};

type Deferred = {
    promise: Promise<StructuredRecordView>;
    resolve: (value: StructuredRecordView) => void;
    reject: (reason: Error) => void;
};

// Every call is left pending unless a test settles it. A provider that answers instantly is the
// one case where none of this matters - the interesting states are the ones where a call is still
// running when the operator does something else.
const deferrals: Deferred[] = [];

const nextDeferral = (): Deferred => {
    let resolve: (value: StructuredRecordView) => void = () => undefined;
    let reject: (reason: Error) => void = () => undefined;

    const promise = new Promise<StructuredRecordView>((resolveIt, rejectIt) => {
        resolve = resolveIt;
        reject = rejectIt;
    });

    // Nothing in the hook attaches a rejection handler until the .catch below it, and an unhandled
    // rejection warning would fail the run.
    promise.catch(() => undefined);

    const deferred = { promise: promise, resolve: resolve, reject: reject };
    deferrals.push(deferred);

    return deferred;
};

const signalOfCall = (callIndex: number): AbortSignal =>
    retrieveStructuredRecordViewAsync.mock.calls[callIndex][1] as AbortSignal;

// vitest runs with globals off, so testing-library's automatic cleanup is never registered and
// every renderHook in this file would otherwise stay mounted for the rest of the run - holding an
// AbortController and a pending promise apiece.
afterEach(cleanup);

beforeEach(() => {
    deferrals.length = 0;
    retrieveStructuredRecordViewAsync.mockReset();
    retrieveStructuredRecordViewAsync.mockImplementation(() => nextDeferral().promise);
});

const submitWith = async (nhsNumber: string) => {
    const rendered = renderHook(() => useStructuredRecordPage());

    act(() => rendered.result.current.handleFieldChange("nhsNumber", nhsNumber));
    act(() => rendered.result.current.handleSubmit());

    return rendered;
};

const submitted = async (nhsNumber: string) => {
    const rendered = await submitWith(nhsNumber);
    await waitFor(() => expect(retrieveStructuredRecordViewAsync).toHaveBeenCalledOnce());

    return rendered;
};

// The plumbing existed through four layers and the one call site never supplied it, so no request
// was cancellable at all.
it("should hand the view service a signal it can be cancelled with", async () => {
    await submitted("9435797881");

    expect(signalOfCall(0)).toBeInstanceOf(AbortSignal);
    expect(signalOfCall(0).aborted).toBe(false);
});

// A structured record is a whole patient bundle against a provider that can take a minute. Leaving
// the page has to abandon it rather than let it land on a component that is gone.
it("should abandon the request in flight when the page unmounts", async () => {
    const rendered = await submitted("9435797881");
    const signal = signalOfCall(0);

    rendered.unmount();

    expect(signal.aborted).toBe(true);
});

it("should abandon the previous request when a second is submitted", async () => {
    const rendered = await submitted("9435797881");

    act(() => rendered.result.current.handleSubmit());
    await waitFor(() => expect(retrieveStructuredRecordViewAsync).toHaveBeenCalledTimes(2));

    expect(signalOfCall(0).aborted).toBe(true);
    expect(signalOfCall(1).aborted).toBe(false);
});

it("should abandon the request in flight when the form is cleared", async () => {
    const rendered = await submitted("9435797881");
    const signal = signalOfCall(0);

    act(() => rendered.result.current.handleClear());

    expect(signal.aborted).toBe(true);
    expect(rendered.result.current.submitting).toBe(false);
});

// The loser of a race must not overwrite the winner, and an abandoned call must not resurrect a
// record on a screen the operator has already cleared.
it("should not show the record of a request that was abandoned", async () => {
    const rendered = await submitted("9435797881");

    act(() => rendered.result.current.handleClear());
    await act(async () => { deferrals[0].resolve(aStructuredRecord); });

    expect(rendered.result.current.structuredRecord).toBeNull();
});

it("should not report an abandoned request as an error the operator must act on", async () => {
    const rendered = await submitted("9435797881");

    act(() => rendered.result.current.handleClear());
    await act(async () => { deferrals[0].reject(new Error("canceled")); });

    expect(rendered.result.current.error).toBeNull();
});

it("should still show the record of a request nobody abandoned", async () => {
    const rendered = await submitted("9435797881");

    await act(async () => { deferrals[0].resolve(aStructuredRecord); });

    expect(rendered.result.current.structuredRecord).toBe(aStructuredRecord);
    expect(rendered.result.current.submitting).toBe(false);
});

it("should still report a genuine failure", async () => {
    const rendered = await submitted("9435797881");
    const failure = new Error("The API answered 400: invalid_client");

    await act(async () => { deferrals[0].reject(failure); });

    expect(rendered.result.current.error).toBe(failure);
    expect(rendered.result.current.submitting).toBe(false);
});

// enableValidationMessages latches on at the first submit and never turns itself off, so without
// disableValidationMessages in handleClear the form comes back blank with an error already on it.
it("should not leave a required-field error on a form the operator just cleared", async () => {
    const rendered = await submitWith("");

    await waitFor(() =>
        expect(rendered.result.current.errors.nhsNumber.length).toBeGreaterThan(0));

    expect(retrieveStructuredRecordViewAsync).not.toHaveBeenCalled();

    act(() => rendered.result.current.handleClear());

    await waitFor(() => expect(rendered.result.current.errors.nhsNumber).toBe(""));
    expect(rendered.result.current.errors.hasErrors).toBe(false);
});
