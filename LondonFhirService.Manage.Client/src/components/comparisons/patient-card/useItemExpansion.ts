// Expansion is held by the page rather than by each row, so opening a list on one side of the
// comparison opens the same list on the other and the two stay lined up.
export function useItemExpansion(
    key: string,
    expandedKeys: Set<string>,
    setExpandedKeys: (expandedKeys: Set<string>) => void)
    : { isExpanded: boolean; toggleExpanded: () => void } {
    const isExpanded = expandedKeys.has(key);

    const toggleExpanded = () => {
        const nextExpandedKeys = new Set(expandedKeys);

        if (isExpanded) {
            nextExpandedKeys.delete(key);
        } else {
            nextExpandedKeys.add(key);
        }

        setExpandedKeys(nextExpandedKeys);
    };

    return { isExpanded: isExpanded, toggleExpanded: toggleExpanded };
}
