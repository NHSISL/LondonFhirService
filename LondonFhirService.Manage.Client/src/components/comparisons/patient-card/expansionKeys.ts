// The structural keys the two cards agree on. Built here rather than inline so both sides
// derive the same string for the same slot, and so a typo in one place cannot silently stop a
// section from mirroring its counterpart.
export const expansionKeys = {
    patientDetails: "patient.details",
    patientAddress: "patient.address",
    managingOrganization: "patient.managingOrganization",

    generalPractitioner: (index: number): string =>
        `patient.generalPractitioner[${index}]`,

    episodeOfCare: (index: number): string => `episodeOfCare[${index}]`,

    // Lists are keyed by title because that is what the comparison itself matches them on - the
    // two providers give the same clinical list different ids.
    list: (title: string): string => `list[${title}]`,

    listItem: (title: string, index: number): string => `list[${title}].item[${index}]`,

    // A reference followed from inside another section - an asserter, an information source, a
    // performer. Named by the slot it fills under its parent rather than by what it points at,
    // for the same reason as everything else here.
    nested: (parentKey: string, slot: string): string => `${parentKey}.${slot}`
};
