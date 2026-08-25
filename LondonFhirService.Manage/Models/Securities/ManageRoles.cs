// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Models.Securities
{
    /// <summary>
    /// The app roles this host authorises against. Each logical role is spelled three ways
    /// because the app registration currently carries three aliases for the same thing - the
    /// intended ManageAdmin/ManageUsers names plus two older forms that are still assigned to
    /// real users. Accepting all three keeps every existing operator working while the
    /// registration is tidied up.
    ///
    /// This is deliberately temporary. When the registration is reduced to one name per role,
    /// delete the alias entries here and nothing else needs to change - every [Authorize] in
    /// this host points at these constants rather than repeating the strings.
    ///
    /// They are const rather than static readonly because [Authorize(Roles = ...)] takes a
    /// compile time constant, which also lets callers append a granular role with +.
    /// </summary>
    public static class ManageRoles
    {
        public const string Administrators =
            "ManageAdmin," +
            "LondonDataServices.Manage.Administrators," +
            "Administrators";

        public const string Users =
            "ManageUsers," +
            "LondonDataServices.Manage.Users," +
            "Users";

        public const string AdministratorsAndUsers = Administrators + "," + Users;
    }
}
