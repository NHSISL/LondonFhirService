import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, expect, it, vi } from "vitest";
import ToggleBase from "./ToggleBase";

// vitest runs with globals off, so testing-library's automatic cleanup is never registered and
// renders would otherwise pile up in the same document.
afterEach(cleanup);

const renderToggle = (checked: boolean, onChange: () => void = () => undefined) =>
    render(
        <ToggleBase
            id="demographicsOnly"
            name="demographicsOnly"
            label="Demographics only"
            checked={checked}
            onChange={onChange} />);

const control = () => screen.getByRole("checkbox") as HTMLInputElement;

const labelsForControl = (container: HTMLElement) =>
    Array.from(container.querySelectorAll('label[for="demographicsOnly"]'))
        .map(label => label.textContent);

// Passing the On/Off word to Form.Check's label prop rendered a second <label for> against the
// same control, so the accessible name came out as "Demographics only On" - and moved to
// "Demographics only Off" when the operator flipped it. A control's name must not change as its
// state changes; that is what the state is for.
it("should be named once, by the label the caller supplied", () => {
    const { container } = renderToggle(true);

    expect(labelsForControl(container)).toEqual(["Demographics only"]);
    expect(screen.getByLabelText("Demographics only")).toBe(control());
});

it("should keep the same name when it is off", () => {
    const { container } = renderToggle(false);

    expect(labelsForControl(container)).toEqual(["Demographics only"]);
    expect(screen.getByLabelText("Demographics only")).toBe(control());
});

it("should report its state through the control rather than through its name", () => {
    renderToggle(true);
    expect(control().checked).toBe(true);

    cleanup();

    renderToggle(false);
    expect(control().checked).toBe(false);
});

// The word stays on screen - it is what a sighted operator reads - but is not part of the name.
it("should keep the on and off wording visible but out of the accessible name", () => {
    const { container } = renderToggle(true);

    expect(container.querySelector('[aria-hidden="true"]')?.textContent).toBe("On");

    cleanup();

    const off = renderToggle(false);
    expect(off.container.querySelector('[aria-hidden="true"]')?.textContent).toBe("Off");
});

it("should report changes to the caller", () => {
    const onChange = vi.fn();
    renderToggle(false, onChange);

    control().click();

    expect(onChange).toHaveBeenCalledOnce();
});
