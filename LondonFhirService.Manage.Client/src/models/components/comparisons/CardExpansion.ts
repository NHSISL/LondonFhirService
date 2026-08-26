// What a card knows about which of its sections are open.
//
// Keys are structural - where a section sits in the card, such as
// "patient.managingOrganization" or "list[Problems].item[2]" - rather than the id of the resource
// it happens to be showing. The two providers mint their own ids for the same clinical fact, so a
// key built from an id could never match its counterpart on the other card, which is exactly why
// expanding one side used to leave the other closed.
export type CardExpansion = {
    isExpanded: (expansionKey: string) => boolean;
    toggleExpanded: (expansionKey: string) => void;
};
