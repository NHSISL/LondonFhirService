// Line alignment for the side by side raw JSON view, the way a merge tool does it: the two
// payloads are lined up so their common lines sit opposite each other, and the lines that do not
// match are marked.
//
// Comparing line 1 to line 1, line 2 to line 2 would not do. One side having a single extra array
// element shifts everything after it, and every remaining line would then read as different.

export type AlignedLine = {
    primaryText: string | null;
    secondaryText: string | null;
    changed: boolean;
};

type Edit = {
    type: "equal" | "removed" | "added";
    primaryIndex: number;
    secondaryIndex: number;
};

// Myers runs in O((N + M) * D), so it is fast when the two payloads mostly agree - the normal
// case here - and slow when they do not. This bounds the bad case: past it the view falls back to
// plain text rather than locking the browser up.
const maximumEditDistance = 1500;

// Beyond this the aligned view would put more elements on the page than it is worth. The plain
// text fallback still shows the whole payload.
const maximumAlignedLines = 12000;

// Returns null when the two are too far apart, or too large, to align within those bounds.
export function alignLines(primaryText: string, secondaryText: string): AlignedLine[] | null {
    const primaryLines = primaryText.split("\n");
    const secondaryLines = secondaryText.split("\n");

    // Two serialisations of the same kind of record share long identical runs at each end.
    // Trimming them first is what keeps the edit distance - and so the work - small.
    let prefixLength = 0;

    while (prefixLength < primaryLines.length
        && prefixLength < secondaryLines.length
        && primaryLines[prefixLength] === secondaryLines[prefixLength]) {
        prefixLength++;
    }

    let suffixLength = 0;

    while (suffixLength < primaryLines.length - prefixLength
        && suffixLength < secondaryLines.length - prefixLength
        && primaryLines[primaryLines.length - 1 - suffixLength]
        === secondaryLines[secondaryLines.length - 1 - suffixLength]) {
        suffixLength++;
    }

    const primaryMiddle = primaryLines.slice(prefixLength, primaryLines.length - suffixLength);
    const secondaryMiddle =
        secondaryLines.slice(prefixLength, secondaryLines.length - suffixLength);

    const edits = diffLines(primaryMiddle, secondaryMiddle);

    if (edits === null) {
        return null;
    }

    const alignedLines: AlignedLine[] = [];

    for (let index = 0; index < prefixLength; index++) {
        alignedLines.push(unchangedLine(primaryLines[index]));
    }

    alignedLines.push(...toAlignedLines(edits, primaryMiddle, secondaryMiddle));

    for (let index = primaryLines.length - suffixLength; index < primaryLines.length; index++) {
        alignedLines.push(unchangedLine(primaryLines[index]));
    }

    return alignedLines.length > maximumAlignedLines ? null : alignedLines;
}

export function countChangedLines(alignedLines: AlignedLine[]): number {
    return alignedLines.filter(alignedLine => alignedLine.changed).length;
}

function unchangedLine(text: string): AlignedLine {
    return { primaryText: text, secondaryText: text, changed: false };
}

// Myers' shortest edit script. The trace of each round is kept so the path can be walked back
// into an edit list once the far corner is reached.
function diffLines(primaryLines: string[], secondaryLines: string[]): Edit[] | null {
    const primaryLength = primaryLines.length;
    const secondaryLength = secondaryLines.length;
    const maximum = Math.min(primaryLength + secondaryLength, maximumEditDistance);
    const offset = maximum + 1;
    const furthestX = new Int32Array(2 * maximum + 3);
    const trace: Int32Array[] = [];

    for (let editDistance = 0; editDistance <= maximum; editDistance++) {
        trace.push(furthestX.slice());

        for (let diagonal = -editDistance; diagonal <= editDistance; diagonal += 2) {
            let x = diagonal === -editDistance
                || (diagonal !== editDistance
                    && furthestX[offset + diagonal - 1] < furthestX[offset + diagonal + 1])
                ? furthestX[offset + diagonal + 1]
                : furthestX[offset + diagonal - 1] + 1;

            let y = x - diagonal;

            while (x < primaryLength
                && y < secondaryLength
                && primaryLines[x] === secondaryLines[y]) {
                x++;
                y++;
            }

            furthestX[offset + diagonal] = x;

            if (x >= primaryLength && y >= secondaryLength) {
                return backtrack(trace, offset, primaryLength, secondaryLength);
            }
        }
    }

    return null;
}

// Walks the recorded rounds back from the far corner, emitting the moves that got there.
function backtrack(
    trace: Int32Array[],
    offset: number,
    primaryLength: number,
    secondaryLength: number)
    : Edit[] {
    const edits: Edit[] = [];
    let x = primaryLength;
    let y = secondaryLength;

    for (let editDistance = trace.length - 1; editDistance >= 0; editDistance--) {
        const furthestX = trace[editDistance];
        const diagonal = x - y;

        const previousDiagonal = diagonal === -editDistance
            || (diagonal !== editDistance
                && furthestX[offset + diagonal - 1] < furthestX[offset + diagonal + 1])
            ? diagonal + 1
            : diagonal - 1;

        const previousX = furthestX[offset + previousDiagonal];
        const previousY = previousX - previousDiagonal;

        while (x > previousX && y > previousY) {
            x--;
            y--;
            edits.push({ type: "equal", primaryIndex: x, secondaryIndex: y });
        }

        if (editDistance === 0) {
            break;
        }

        if (x === previousX) {
            y--;
            edits.push({ type: "added", primaryIndex: x, secondaryIndex: y });
        } else {
            x--;
            edits.push({ type: "removed", primaryIndex: x, secondaryIndex: y });
        }
    }

    return edits.reverse();
}

// A run of removals followed by a run of additions is one changed block, so they are put opposite
// each other rather than one after the other. Where the runs are uneven the shorter side gets
// blank rows, which is what keeps the rest of the two payloads lined up.
function toAlignedLines(
    edits: Edit[],
    primaryLines: string[],
    secondaryLines: string[])
    : AlignedLine[] {
    const alignedLines: AlignedLine[] = [];
    let removedTexts: string[] = [];
    let addedTexts: string[] = [];

    const flushChangedBlock = () => {
        const blockLength = Math.max(removedTexts.length, addedTexts.length);

        for (let index = 0; index < blockLength; index++) {
            alignedLines.push({
                primaryText: removedTexts[index] ?? null,
                secondaryText: addedTexts[index] ?? null,
                changed: true
            });
        }

        removedTexts = [];
        addedTexts = [];
    };

    for (const edit of edits) {
        if (edit.type === "equal") {
            flushChangedBlock();
            alignedLines.push(unchangedLine(primaryLines[edit.primaryIndex]));
        } else if (edit.type === "removed") {
            removedTexts.push(primaryLines[edit.primaryIndex]);
        } else {
            addedTexts.push(secondaryLines[edit.secondaryIndex]);
        }
    }

    flushChangedBlock();

    return alignedLines;
}
