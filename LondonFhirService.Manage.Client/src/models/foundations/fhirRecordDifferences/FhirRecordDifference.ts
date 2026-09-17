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

    // From $expand=Secondary($select=SourceName,IsPrimarySource) - the provider whose answer this
    // row compared, without the FhirRecord it came from. Empty and false when the expand was not
    // asked for or the record has gone, which the list renders as a dash rather than a blank.
    secondarySourceName: string;
    secondaryIsPrimarySource: boolean;
    createdBy: string;
    createdDate: string;
    updatedBy: string;
    updatedDate: string;
};
