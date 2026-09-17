// The app registration carries one name per logical role - Administrators and Users - and these
// two arrays are where the client writes them down. The STRINGS mirror the ManageRoles constants
// the Manage host authorises against; change one and change the other.
//
// The strings matching does not mean the POLICY matches. Which roles each area below grants is
// maintained by hand against each controller's [Authorize], and two areas are currently out of
// step with the server - see the notes on audits and comparisons. Nothing here enforces
// anything: this matrix decides what the portal shows, the attributes decide what the API
// allows, and only the second is a security boundary.
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
    // and unroutable - so only view is granted.
    //
    // MISMATCH, left as found and not silently changed: this grants Administrators AND Users,
    // but AuditsController carries [Authorize(Roles = ManageRoles.Administrators)]. A Users-role
    // operator is therefore shown the audit screen and gets 403 from every call it makes. The
    // safe direction to reconcile is tightening this array to administratorRoles, but which side
    // is wrong is a policy question - an audit row carries a whole patient payload - so it is
    // the repository owner's call, not a tidy-up.
    audits: {
        view: administratorAndUserRoles,
    },
    // Metrics carry no patient identifiable data by design, and the API's write verbs are
    // [InvisibleApi] and unroutable, so this is view only to the same audience
    // MetricsController allows.
    metrics: {
        view: administratorAndUserRoles,
    },
    // A comparison holds two whole patient bundles, so this area is administrators only. Edit
    // covers the review fields an operator can set on a comparison; the differences themselves are
    // written by the comparison service and are not editable from here, so there is no add or
    // delete.
    //
    // MISMATCH, left as found and not silently changed, and the more serious of the two: the UI
    // is STRICTER than the API. FhirRecordsController and FhirRecordDifferencesController are
    // [Authorize(Roles = ManageRoles.AdministratorsAndUsers)], so a Users-role operator cannot
    // see this area in the portal but can call those endpoints directly and read the patient
    // bundles behind it. Hiding a screen is not authorisation. Reconciling means either widening
    // this array or narrowing those two controllers, and that decides who can read patient data -
    // the repository owner's call.
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
