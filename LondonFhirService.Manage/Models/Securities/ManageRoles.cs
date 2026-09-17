// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Models.Securities
{
    /// <summary>
    /// The app roles this host authorises against. The app registration carries one name per
    /// logical role - Administrators and Users - and these constants are the single place those
    /// two strings are written down, so every [Authorize] in this host points here rather than
    /// repeating a literal that nothing would catch if it drifted from the registration.
    ///
    /// AdministratorsAndUsers is just the pair joined the way the attribute expects: a comma
    /// separated list where holding any one of the named roles is enough to pass.
    ///
    /// They are const rather than static readonly because [Authorize(Roles = ...)] takes a
    /// compile time constant, which also lets callers append a granular role with +.
    /// </summary>
    public static class ManageRoles
    {
        public const string Administrators = "Administrators";

        public const string Users = "Users";

        public const string AdministratorsAndUsers = Administrators + "," + Users;
    }
}
