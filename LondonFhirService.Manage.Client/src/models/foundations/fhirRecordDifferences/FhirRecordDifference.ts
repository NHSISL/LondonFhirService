// Foundation model - mirrors
// LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.FhirRecordDifference as it is
// serialised by the /api/fhirrecorddifferences endpoint. No UI concerns live here.
//
// DiffJson is a serialised ComparisonResult - see models/foundations/comparisons - written by the
// comparison coordination service when the two records were compared.
export type FhirRecordDifference = {
    id: string;
    primaryId: string;
    secondaryId: string;
    correlationId: string;
    diffJson: string;
    diffCount: number;
    acceptableDiffCount: number;
    comparedAt: string;
    comment: string | null;
    isResolved: boolean;
    createdBy: string;
    createdDate: string;
    updatedBy: string;
    updatedDate: string;
};
