import type { FhirResource } from "./FhirResource";

export type PatientData = {
    nhsNumber: string | null;
    namePrefix: string | null;
    nameGiven: string | null;
    nameFamily: string | null;
    nameSuffix: string | null;
    birthDate: string | null;
    gender: string | null;
    addressLine: string | null;
    addressCity: string | null;
    addressDistrict: string | null;
    addressPostalCode: string | null;
    addressCountry: string | null;
    telecom: string | null;
    communication: string | null;
    managingOrganizationRef: string | null;
    generalPractitionerRefs: string[];
    resource: FhirResource | null;
};
