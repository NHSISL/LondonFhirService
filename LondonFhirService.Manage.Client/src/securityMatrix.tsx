// The app registration carries one name per logical role - Administrators and Users - and these
// two arrays are where the client writes them down. They mirror the ManageRoles constants the
// Manage host authorises against, so a screen the client shows and an endpoint the host allows
// are gated on the same strings; change one and change the other.
const administratorRoles = ['Administrators'];

const userRoles = ['Users'];

const administratorAndUserRoles = [...administratorRoles, ...userRoles];

const securityPoints = {
    configuration: {
        add: administratorAndUserRoles,
        edit: administratorAndUserRoles,
        delete: administratorAndUserRoles,
        view: administratorAndUserRoles,
    },
    // The audit trail is read only in this portal - the API's write verbs are [InvisibleApi]
    // and unroutable - so only view is granted, to the same audience AuditsController allows.
    audits: {
        view: administratorAndUserRoles,
    },
    // Metrics carry no patient identifiable data by design, and the API's write verbs are
    // [InvisibleApi] and unroutable, so this is view only to the same audience
    // MetricsController allows.
    metrics: {
        view: administratorAndUserRoles,
    },
    // A comparison holds two whole patient bundles, so the area is administrators only - the same
    // audience FhirRecordDifferencesController and FhirRecordsController authorise against. Edit
    // covers the review fields an operator can set on a comparison; the differences themselves are
    // written by the comparison service and are not editable from here, so there is no add or
    // delete.
    comparisons: {
        edit: administratorRoles,
        view: administratorRoles,
    },
    // The provider registry decides who the patient fan-out calls, so the whole area - the master
    // list as well as the detail view - is administrators only.
    providers: {
        add: administratorRoles,
        edit: administratorRoles,
        delete: administratorRoles,
        view: administratorRoles,
    },
    testUserAction: {
        add: administratorAndUserRoles,
        edit: administratorAndUserRoles,
        delete: administratorAndUserRoles,
        view: administratorAndUserRoles,
    }
}

export default securityPoints
