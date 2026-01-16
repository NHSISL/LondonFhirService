const securityPoints = {
    configuration: {
        add: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices.Manage.Users'],
        edit: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices.Manage.Users'],
        delete: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices.Manage.Users'],
        view: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices..Manage.Users'],
    },
    testUserAction: {
        add: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices.Manage.Users'],
        edit: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices.Manage.Users'],
        delete: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices.Manage.Users'],
        view: ['LondonFhirServices.Manage.Administrators', 'LondonFhirServices.Manage.Users'],
    }
}

export default securityPoints