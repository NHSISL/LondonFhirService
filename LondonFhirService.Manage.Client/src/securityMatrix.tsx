// Role names must match the app roles on the Manage app registration, which are the same values
// the Manage host authorises against in its controllers: ManageAdmin and ManageUsers.
const securityPoints = {
    configuration: {
        add: ['ManageAdmin', 'ManageUsers'],
        edit: ['ManageAdmin', 'ManageUsers'],
        delete: ['ManageAdmin', 'ManageUsers'],
        view: ['ManageAdmin', 'ManageUsers'],
    },
    // The provider registry decides who the patient fan-out calls, so the whole area - the master
    // list as well as the detail view - is ManageAdmin only.
    providers: {
        add: ['ManageAdmin'],
        edit: ['ManageAdmin'],
        delete: ['ManageAdmin'],
        view: ['ManageAdmin'],
    },
    testUserAction: {
        add: ['ManageAdmin', 'ManageUsers'],
        edit: ['ManageAdmin', 'ManageUsers'],
        delete: ['ManageAdmin', 'ManageUsers'],
        view: ['ManageAdmin', 'ManageUsers'],
    }
}

export default securityPoints
