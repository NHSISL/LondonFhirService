// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Attrify.InvisibleApi.Models;
using Microsoft.AspNetCore.Authentication;

namespace LondonFhirService.Manage.Tests.Acceptance
{
    public class CustomAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public InvisibleApiKey InvisibleApiKey { get; set; }
    }
}
