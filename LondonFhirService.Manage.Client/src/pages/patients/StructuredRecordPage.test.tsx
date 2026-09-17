import { act, cleanup, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, expect, it, vi } from "vitest";
import { StructuredRecordPage } from "./StructuredRecordPage";
import type { StructuredRecordView } from "../../models/views/patients/StructuredRecordView";
import {
    StructuredRecordViewServiceException
} from "../../models/views/patients/exceptions/StructuredRecordViewServiceException";

const retrieveStructuredRecordViewAsync = vi.fn();

vi.mock("../../services/views/patients/structuredRecordViewService", () => ({
    StructuredRecordViewService: class {
        createStructuredRecordFormValues() {
            return {
                clientId: "",
                clientSecret: "",
                scope: "",
                grantType: "client_credentials",
                nhsNumber: "9435797881",
                dateOfBirth: "",
                demographicsOnly: false
            };
        }

        retrieveStructuredRecordViewAsync(values: unknown, abortSignal?: AbortSignal) {
            return retrieveStructuredRecordViewAsync(values, abortSignal);
        }
    }
}));

const aStructuredRecord: StructuredRecordView = {
    payloadText: "{\n  \"resourceType\": \"Bundle\"\n}",
    isJson: true,
    lineCount: 3,
    characterCountText: "30 characters",
    correlationId: "9f2c41be7a0d4e5bb6c8d3117e42a905",
    metricsUrl: "/admin/metrics/9f2c41be-7a0d-4e5b-b6c8-d3117e42a905",
    comparisonsUrl: "/admin/comparisons?correlationId=9f2c41be7a0d4e5bb6c8d3117e42a905"
};

// Neither happy-dom nor jsdom implements scrollIntoView, and the page calls it. Recorded rather
// than merely silenced, because whether it was called is the thing under test.
const scrollIntoView = vi.fn();

let settle: (value: StructuredRecordView) => void;
let fail: (reason: Error) => void;

// Restored in teardown rather than at the end of each body. An assertion that throws never
// reaches an inline restore, so one real failure left a prefers-reduced-motion stub installed and
// reported a second defect in the next test that did not exist.
const originalScrollIntoView = Element.prototype.scrollIntoView;

afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
    Element.prototype.scrollIntoView = originalScrollIntoView;
});

beforeEach(() => {
    Element.prototype.scrollIntoView = scrollIntoView;
    scrollIntoView.mockClear();
    retrieveStructuredRecordViewAsync.mockReset();

    retrieveStructuredRecordViewAsync.mockImplementation(() =>
        new Promise<StructuredRecordView>((resolveIt, rejectIt) => {
            settle = resolveIt;
            fail = rejectIt;
        }).catch(reason => {
            throw reason;
        }));
});

// The payload card links its correlation id into the comparisons list, so the page needs a
// router around it even though no test here navigates.
const submit = async () => {
    render(<MemoryRouter><StructuredRecordPage /></MemoryRouter>);
    const submitButton = screen.getByRole("button", { name: /get structured record/i });

    await act(async () => { submitButton.click(); });
    await waitFor(() => expect(retrieveStructuredRecordViewAsync).toHaveBeenCalledOnce());
};

const outcomeRegion = () => screen.getByLabelText("Result");

// TextInputBase wraps its own input in a div, so a field's message is a sibling of that wrapper
// rather than inside it. The enclosing column is the nearest element holding both.
const fieldColumn = (label: RegExp) =>
    screen.getByLabelText(label).closest("[class*='col-']");

// The request used to look like it did nothing: the form is taller than the viewport, so whatever
// rendered below it was off screen at the moment it appeared.
it("should move to the result area as soon as the request starts", async () => {
    await submit();

    expect(scrollIntoView).toHaveBeenCalled();
    expect(screen.getByText(/retrieving the structured record/i)).toBeTruthy();
});

// Focus rather than only scroll, so the outcome is announced and not merely displayed.
it("should focus the result when the record arrives", async () => {
    await submit();
    await act(async () => { settle(aStructuredRecord); });

    await waitFor(() => expect(document.activeElement).toBe(outcomeRegion()));
});

// The error summary used to render above the form, so a failure appeared off screen upwards -
// the one outcome where nothing visible happened at all.
it("should focus the result when the request fails", async () => {
    await submit();
    await act(async () => { fail(new Error("The API answered 400: invalid_client")); });

    await waitFor(() => expect(document.activeElement).toBe(outcomeRegion()));
    expect(outcomeRegion().textContent).toContain("invalid_client");
});

// preventScroll, so the browser's own jump to the focused element does not fight the smooth
// scroll that follows it.
it("should not let focusing the result scroll the page out from under the animation", async () => {
    const focus = vi.spyOn(HTMLDivElement.prototype, "focus");

    await submit();
    await act(async () => { settle(aStructuredRecord); });

    await waitFor(() => expect(focus).toHaveBeenCalledWith({ preventScroll: true }));
});

// A long page travelling under someone who gets motion sick from it is not a courtesy.
it("should not animate the scroll when the reader asked for no motion", async () => {
    vi.stubGlobal("matchMedia", (query: string) => ({
        matches: query.includes("prefers-reduced-motion"),
        media: query,
        addEventListener: () => undefined,
        removeEventListener: () => undefined
    }));

    await submit();

    expect(scrollIntoView).toHaveBeenCalledWith({ behavior: "auto", block: "start" });
});

it("should animate the scroll otherwise", async () => {
    await submit();

    expect(scrollIntoView).toHaveBeenCalledWith({ behavior: "smooth", block: "start" });
});

// A deployed environment deliberately configures no consumer credential - the operator is
// expected to supply one on this form - so the API rejecting a blank clientId is the ordinary
// case here rather than a fault. It used to arrive as "clientId: Text is invalid" flattened into
// the error banner, nowhere near the box to type it in.
it("should flag a rejected credential under its own field", async () => {
    await submit();

    await act(async () => {
        fail(new StructuredRecordViewServiceException(
            "Some of the details below need correcting.",
            null,
            {
                clientId: ["Text is invalid"],
                clientSecret: ["Text is invalid"]
            }));
    });

    // Scoped to each field's own column rather than the document, so the test would still fail
    // if both messages landed in one place.
    await waitFor(() =>
        expect(fieldColumn(/client id/i)?.textContent).toContain("Text is invalid"));

    expect(fieldColumn(/client secret/i)?.textContent).toContain("Text is invalid");
});

// Repeating them in the banner as well would say the same thing twice, once where the operator
// can act on it and once where they cannot.
it("should not repeat field errors in the result banner", async () => {
    await submit();

    await act(async () => {
        fail(new StructuredRecordViewServiceException(
            "Some of the details below need correcting.",
            null,
            { clientId: ["Text is invalid"] }));
    });

    await waitFor(() =>
        expect(fieldColumn(/client id/i)?.textContent).toContain("Text is invalid"));

    expect(outcomeRegion().textContent).not.toContain("Text is invalid");
});

// A setting the environment is missing has no input to sit under, so it has to stay in the
// banner - silence would leave an operator retyping credentials against a host that could never
// have answered.
it("should keep a configuration failure in the banner", async () => {
    await submit();

    await act(async () => {
        fail(new StructuredRecordViewServiceException(
            "We could not retrieve the structured record. This environment is not fully "
            + "configured for it - getStructuredRecordUrl: Text must be a valid absolute http "
            + "or https url.",
            null,
            {}));
    });

    await waitFor(() =>
        expect(outcomeRegion().textContent).toContain("getStructuredRecordUrl"));
});
