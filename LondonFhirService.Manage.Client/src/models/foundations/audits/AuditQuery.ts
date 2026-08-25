// The paging and search window a list request asks the API for.
export type AuditQuery = {
    skip: number;
    take: number;
    searchTerm: string;
};
