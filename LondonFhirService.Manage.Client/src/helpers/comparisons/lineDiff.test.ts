import { expect, it } from "vitest";
import { alignLines, countChangedLines } from "./lineDiff";

const align = (primaryText: string, secondaryText: string) => {
    const alignedLines = alignLines(primaryText, secondaryText);

    expect(alignedLines).not.toBeNull();

    return alignedLines ?? [];
};

it("should mark nothing when the two payloads are identical", () => {
    const alignedLines = align("a\nb\nc", "a\nb\nc");

    expect(alignedLines).toHaveLength(3);
    expect(countChangedLines(alignedLines)).toBe(0);
});

it("should mark only the line that changed", () => {
    const alignedLines = align("a\nb\nc", "a\nB\nc");

    expect(alignedLines).toHaveLength(3);
    expect(alignedLines[1]).toEqual({ primaryText: "b", secondaryText: "B", changed: true });
    expect(countChangedLines(alignedLines)).toBe(1);
});

// The whole point of aligning rather than comparing line by line: an extra element on one side
// shifts everything after it, and a positional comparison would call every later line different.
it("should keep the lines after an insertion lined up", () => {
    const alignedLines = align("a\nb\nc", "a\nextra\nb\nc");

    expect(alignedLines.map(line => line.primaryText)).toEqual(["a", null, "b", "c"]);
    expect(alignedLines.map(line => line.secondaryText)).toEqual(["a", "extra", "b", "c"]);
    expect(countChangedLines(alignedLines)).toBe(1);
});

it("should leave a blank opposite a line only the primary has", () => {
    const alignedLines = align("a\ngone\nb", "a\nb");

    expect(alignedLines.map(line => line.primaryText)).toEqual(["a", "gone", "b"]);
    expect(alignedLines.map(line => line.secondaryText)).toEqual(["a", null, "b"]);
});

// A changed block puts its removals opposite its additions rather than one run after the other,
// so a reader can see what became what.
it("should put a changed block side by side", () => {
    const alignedLines = align("a\none\ntwo\nb", "a\nuno\ndos\nb");

    expect(alignedLines.map(line => line.primaryText)).toEqual(["a", "one", "two", "b"]);
    expect(alignedLines.map(line => line.secondaryText)).toEqual(["a", "uno", "dos", "b"]);
    expect(countChangedLines(alignedLines)).toBe(2);
});

it("should align a realistic pair of payloads on the value that differs", () => {
    const primaryText = JSON.stringify(
        { resourceType: "Patient", id: "1", gender: "male" }, null, 2);

    const secondaryText = JSON.stringify(
        { resourceType: "Patient", id: "2", gender: "male" }, null, 2);

    const alignedLines = align(primaryText, secondaryText);
    const changedLines = alignedLines.filter(line => line.changed);

    expect(changedLines).toHaveLength(1);
    expect(changedLines[0].primaryText).toContain("\"id\": \"1\"");
    expect(changedLines[0].secondaryText).toContain("\"id\": \"2\"");
});

it("should give up rather than grind on two payloads with nothing in common", () => {
    const primaryText = Array.from({ length: 4000 }, (_unused, index) => `a${index}`).join("\n");
    const secondaryText = Array.from({ length: 4000 }, (_unused, index) => `b${index}`).join("\n");

    expect(alignLines(primaryText, secondaryText)).toBeNull();
});

it("should handle one side being empty", () => {
    const alignedLines = align("", "a\nb");

    expect(alignedLines.map(line => line.secondaryText)).toEqual(["a", "b"]);
    expect(countChangedLines(alignedLines)).toBe(2);
});
