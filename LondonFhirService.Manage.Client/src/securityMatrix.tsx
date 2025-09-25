const securityPoints = {
    configuration: {
        add: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
        edit: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
        delete: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
        view: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
    },
    lookups: {
        add: ['LondonDataServices.IDecide.Manage.Administrators'],
        edit: ['LondonDataServices.IDecide.Manage.Administrators'],
        delete: ['LondonDataServices.IDecide.Manage.Administrators'],
        view: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
    },
    patientSearch: {
        add: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
        edit: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
        delete: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
        view: ['LondonDataServices.IDecide.Manage.Administrators', 'LondonDataServices.IDecide.Manage.Users'],
    },

}

export default securityPoints