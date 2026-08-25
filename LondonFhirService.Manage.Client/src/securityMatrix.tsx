// The app registration currently carries three aliases for each logical role - the intended
// ManageAdmin/ManageUsers names plus two older forms that are still assigned to real users. All
// three are accepted so every existing operator keeps working, and they mirror the ManageRoles
// constants the Manage host authorises against.
//
// This is deliberately temporary. Once the registration is reduced to one name per role, drop the
// alias entries from these arrays and from ManageRoles.cs.
const administratorRoles = [
    'ManageAdmin',
    'LondonDataServices.Manage.Administrators',
    'Administrators',
];

const userRoles = [
    'ManageUsers',
    'LondonDataServices.Manage.Users',
    'Users',
];

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
