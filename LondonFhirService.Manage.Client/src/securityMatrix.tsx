// The app registration carries one name per logical role - Administrators and Users - and these
// two arrays are where the client writes them down. The STRINGS mirror the ManageRoles constants
// the Manage host authorises against; change one and change the other.
//
// The strings matching does not mean the POLICY matches. Which roles each area below grants is
// maintained by hand against each controller's [Authorize]. Nothing here enforces anything: this
// matrix decides what the portal shows, the attributes decide what the API allows, and only the
// second is a security boundary.
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
    // Administrators only, matching AuditsController's [Authorize]. This used to grant users as
    // well, which put the menu item in front of an audience every call behind it answers 403 to.
    // Reconciled towards the server rather than away from it - an audit row carries a whole
    // patient payload, so the narrower side is the one to keep.
    audits: {
        view: administratorRoles,
    },
    // Metrics carry no patient identifiable data by design, and the API's write verbs are
    // [InvisibleApi] and unroutable, so this is view only to the same audience
    // MetricsController allows.
    metrics: {
        view: administratorAndUserRoles,
    },
    // The page calls $getstructuredrecord live and shows a whole patient record, so it is granted
    // to the same audience PatientsController authorises - administrators and users. Unlike the
    // comparison area it stores nothing: the record is fetched for the screen and gone when the
    // operator leaves it.
    structuredRecord: {
        view: administratorAndUserRoles,
    },
    // Administrators and Users, matching the server. Edit covers the review fields an operator
    // can set on a comparison; the differences themselves are written by the comparison service
    // and are not editable from here, so there is no add or delete.
    //
    // This area used to be administrators only while FhirRecordsController and
    // FhirRecordDifferencesController both carry
    // [Authorize(Roles = ManageRoles.AdministratorsAndUsers)] - so the portal hid a screen whose
    // endpoints a Users-role operator could call directly, which reads as a control but is not
    // one. Hiding a screen is not authorisation; the attributes are the boundary. Widened
    // deliberately rather than narrowing the controllers, so the two now agree.
    comparisons: {
        edit: administratorAndUserRoles,
        view: administratorAndUserRoles,
    },
    // Split the way ProvidersController splits, because that is the boundary. A provider row
    // decides who the patient fan-out calls and which source is primary, so writing one changes
    // what every consumer gets back - create, update and delete are Administrators only, matching
    // the method-level attributes.
    //
    // Reading one is not that. The controller's class-level attribute opens both GETs to
    // Administrators and Users, and a provider carries no credential and no patient data - a
    // friendly name, a fully qualified name, a FHIR version, and when it is active. This used to
    // grant view to administrators only, which hid the screen from an audience that could call
    // GET /api/Providers directly and get the same rows. That reads as a control and is not one;
    // hiding a screen has never been authorisation.
    providers: {
        add: administratorRoles,
        edit: administratorRoles,
        delete: administratorRoles,
        view: administratorAndUserRoles,
    },
    testUserAction: {
        add: administratorAndUserRoles,
        edit: administratorAndUserRoles,
        delete: administratorAndUserRoles,
        view: administratorAndUserRoles,
    }
}

export default securityPoints
