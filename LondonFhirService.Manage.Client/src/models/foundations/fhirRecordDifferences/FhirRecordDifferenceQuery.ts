// The paging and search window a comparison list request asks the API for.
export type FhirRecordDifferenceQuery = {
    skip: number;
    take: number;
    searchTerm: string;
    unresolvedOnly: boolean;
};
