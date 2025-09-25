// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;

namespace LondonFhirService.Core.Brokers.Securities
{
    public interface ISecurityBroker
    {
        ValueTask<User> GetCurrentUserAsync();
        ValueTask<bool> IsCurrentUserAuthenticatedAsync();
        ValueTask<bool> IsInRoleAsync(string roleName);
        ValueTask<bool> HasClaimAsync(string claimType, string claimValue);
        ValueTask<bool> HasClaimAsync(string claimType);
        ValueTask<bool> ValidateCaptchaAsync();
        ValueTask<string> GetIpAddressAsync();
    }
}
